using DragonSpark.Activation;
using DragonSpark.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IProfile> DefaultProfiles { get; } = new IProfile[] { GenericFactoryProfile.Instance, GenericCommandProfile.Instance, FactoryProfile.Instance, CommandProfile.Instance }.ToImmutableArray();

		class GenericFactoryProfile : Profile
		{
			public static GenericFactoryProfile Instance { get; } = new GenericFactoryProfile();
			GenericFactoryProfile() : base( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), GenericFactoryProfileFactory.Instance ) {}
		}

		class FactoryProfile : Profile
		{
			public static FactoryProfile Instance { get; } = new FactoryProfile();
			FactoryProfile() : base( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), FactoryProfileFactory.Instance ) {}
		}

		class GenericCommandProfile : Profile
		{
			public static GenericCommandProfile Instance { get; } = new GenericCommandProfile();
			GenericCommandProfile() : base( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute), GenericCommandProfileFactory.Instance ) {}
		}

		class CommandProfile : Profile
		{
			public static CommandProfile Instance { get; } = new CommandProfile();
			CommandProfile() : base( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute), CommandProfileFactory.Instance ) {}
		}
	}
	
	class AdapterLocator : FactoryBase<object, ParameterInstanceProfile>
	{
		public static AdapterLocator Instance { get; } = new AdapterLocator();
		readonly Func<Type, Func<object, ParameterInstanceProfile>> factorySource;
		
		AdapterLocator() : this( AutoValidation.DefaultProfiles ) {}

		public AdapterLocator( ImmutableArray<IProfile> profiles ) : this( new Factory( profiles ).Cached().Get ) {}

		AdapterLocator( Func<Type, Func<object, ParameterInstanceProfile>> factorySource )
		{
			this.factorySource = factorySource;
		}

		class Factory : FactoryBase<Type, Func<object, ParameterInstanceProfile>>
		{
			readonly ImmutableArray<IProfile> profiles;
			public Factory( ImmutableArray<IProfile> profiles )
			{
				this.profiles = profiles;
			}

			public override Func<object, ParameterInstanceProfile> Create( Type parameter )
			{
				foreach ( var profile in profiles )
				{
					if ( profile.InterfaceType.IsAssignableFrom( parameter ) )
					{
						return profile.ProfileSource;
					}
				}
				return null;
			}
		}

		public override ParameterInstanceProfile Create( object parameter )
		{
			var other = parameter.GetType();
			var factory = factorySource( other );
			if ( factory != null )
			{
				return factory( parameter );
			}

			throw new InvalidOperationException( $"Profile not found for {other}." );
		}
	}

	public interface IAutoValidationController
	{
		bool? IsValid( object parameter );

		void MarkValid( object parameter, bool valid );

		AutoValidationControllerResult Execute( object parameter, out object result );
	}

	public enum AutoValidationControllerResult { None, ResultFound, Proceed }

	public interface IMethodAwareAspect
	{
		MethodBase Method { get; }
	}

	public interface IAspectHub
	{
		void Register( IAspect aspect );
	}

	class AutoValidationController : ConcurrentDictionary<int, object>, IAutoValidationController, IAspectHub
	{
		readonly IParameterValidationAdapter validator;
		readonly IParameterAwareHandler handler;

		public AutoValidationController( IParameterValidationAdapter validator, IParameterAwareHandler handler )
		{
			this.validator = validator;
			this.handler = handler;
		}

		public bool? IsValid( object parameter ) => handler.Handles( parameter ) ? true : CheckValid( parameter );

		bool? CheckValid( object parameter )
		{
			object current;
			return TryGetValue( Environment.CurrentManagedThreadId, out current ) ? (bool?)Equals( current, parameter ) : null;
		}

		public void MarkValid( object parameter, bool valid )
		{
			if ( valid )
			{
				this[Environment.CurrentManagedThreadId] = parameter;
			}
			else
			{
				object removed;
				TryRemove( Environment.CurrentManagedThreadId, out removed );
			}
		}

		public AutoValidationControllerResult Execute( object parameter, out object result )
		{
			var status = 
				handler.Handle( parameter, out result ) ? AutoValidationControllerResult.ResultFound 
				: 
				CheckValid( parameter ).GetValueOrDefault() || validator.IsValid( parameter ) ? AutoValidationControllerResult.Proceed : AutoValidationControllerResult.None;
			MarkValid( parameter, false );
			return status;
		}

		// readonly ISet<>

		public void Register( IAspect aspect )
		{
			
		}
	}
}
