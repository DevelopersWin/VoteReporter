using DragonSpark.Activation;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;

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
	
	class AdapterLocator : FactoryBase<object, IParameterValidationAdapter>
	{
		public static AdapterLocator Instance { get; } = new AdapterLocator();
		AdapterLocator() : this( AutoValidation.DefaultProfiles ) {}

		readonly Func<Type, Func<object, IParameterValidationAdapter>> factorySource;

		public AdapterLocator( ImmutableArray<IProfile> profiles ) : this( new Factory( profiles ).Cached().Get ) {}

		AdapterLocator( Func<Type, Func<object, IParameterValidationAdapter>> factorySource )
		{
			this.factorySource = factorySource;
		}

		class Factory : FactoryBase<Type, Func<object, IParameterValidationAdapter>>
		{
			readonly ImmutableArray<IProfile> profiles;
			public Factory( ImmutableArray<IProfile> profiles )
			{
				this.profiles = profiles;
			}

			public override Func<object, IParameterValidationAdapter> Create( Type parameter )
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

		public override IParameterValidationAdapter Create( object parameter )
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
		bool? IsValid( object parameter, Func<bool> proceed );

		// void MarkValid( object parameter, bool valid );

		object Execute( object parameter, Func<object> proceed );
	}

	// public enum AutoValidationControllerResult { None, ResultFound, Proceed }

	public interface IMethodAware
	{
		MethodBase Method { get; }
	}

	public interface IAspectHub
	{
		void Register( IAspect aspect );
	}

	/*interface IParameterAwareHandlerRegistry : IParameterAwareHandler
	{
		void Register( IParameterAwareHandler handler );
	}*/

	class LinkedParameterAwareHandler : IParameterAwareHandler
	{
		readonly IParameterAwareHandler current;
		readonly IParameterAwareHandler next;

		public LinkedParameterAwareHandler( IParameterAwareHandler current, IParameterAwareHandler next )
		{
			this.current = current;
			this.next = next;
		}

		public bool Handles( object parameter ) => current.Handles( parameter ) || next.Handles( parameter );

		public bool Handle( object parameter, out object handled ) => current.Handle( parameter, out handled ) || next.Handle( parameter, out handled );
	}

	class AutoValidationController : ConcurrentDictionary<int, object>, IAutoValidationController, IAspectHub
	{
		readonly IParameterValidationAdapter validator;
		
		public AutoValidationController( IParameterValidationAdapter validator )
		{
			this.validator = validator;
		}

		IParameterAwareHandler Handler { get; set; }

		public bool? IsValid( object parameter, Func<bool> proceed )
		{
			var result = CheckValid( parameter );
			if ( !result.HasValue )
			{
				MarkValid( parameter, (bool)( result = proceed() ) );
			}
			return result;
		}

		bool? CheckValid( object parameter )
		{
			object current;
			return TryGetValue( Environment.CurrentManagedThreadId, out current ) ? (bool?)Equals( current, parameter ) : null;
		}

		void MarkValid( object parameter, bool valid )
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

		public object Execute( object parameter, Func<object> proceed )
		{
			var valid = CheckValid( parameter ).GetValueOrDefault() || validator.IsValid( parameter );
			var result = valid ? proceed() : null;
			MarkValid( parameter, false );
			return result;

			/*result = null;
			var status = 
				/*( Handler != null && Handler.Handle( parameter, out result ) ) ? AutoValidationControllerResult.ResultFound 
				: #1#
				CheckValid( parameter ).GetValueOrDefault() || validator.IsValid( parameter ) ? AutoValidationControllerResult.Proceed : AutoValidationControllerResult.None;
			MarkValid( parameter, false );
			return status;*/
		}

		// readonly ISet<>

		public void Register( IAspect aspect )
		{
			/*var methodAware = aspect as IMethodAware;
			if ( methodAware != null && Equals( methodAware.Method, validator.Method ) )
			{
				var handler = aspect as IParameterAwareHandler;
				if ( handler != null )
				{
					Handler = Handler != null ? new LinkedParameterAwareHandler( handler, Handler ) : handler;
				}
			}*/
		}
	}
}
