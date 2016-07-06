using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IProfile> DefaultProfiles { get; } = new IProfile[] { GenericFactoryProfile.Instance, FactoryProfile.Instance, GenericCommandProfile.Instance, CommandProfile.Instance }.ToImmutableArray();

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

	public class ParameterHandlerLocator : FactoryBase<Delegate, IParameterAwareHandler>
	{
		public static ParameterHandlerLocator Instance { get; } = new ParameterHandlerLocator();

		readonly IDelegateParameterHandlerRegistry registry;
		readonly Func<Delegate, Delegate> lookup;

		ParameterHandlerLocator() : this( DelegateParameterHandlerRegistry.Instance, Delegates.Default.Lookup ) {}

		public ParameterHandlerLocator( IDelegateParameterHandlerRegistry registry, Func<Delegate, Delegate> lookup )
		{
			this.registry = registry;
			this.lookup = lookup;
		}

		public override IParameterAwareHandler Create( Delegate parameter )
		{
			var key = lookup( parameter );
			var result = key != null ? From( registry.Get( key ) ) : ParameterAwareHandler.Instance;
			return result;
		}

		static IParameterAwareHandler From( ImmutableArray<IParameterAwareHandler> handlers ) => handlers.ToArray().Only() ?? new CompositeParameterAwareHandler( handlers );
	}

	public enum AutoValidationControllerResult { None, ResultFound, Proceed }

	class AutoValidationController : ArgumentCache<int, object>, IAutoValidationController
	{
		readonly IParameterValidationAdapter validator;
		
		public AutoValidationController( IParameterValidationAdapter validator/*, IParameterAwareHandler handler*/ )
		{
			this.validator = validator;
		}

		// public new bool? IsValid( object parameter ) => /*handler.Handles( parameter ) ? true :*/ base.IsValid( parameter );
		public bool? IsValid( object parameter )
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
			result = null;
			var status = 
				/*handler.Handle( parameter, out result ) ? AutoValidationControllerResult.ResultFound 
				: */
				IsValid( parameter ).GetValueOrDefault() || validator.IsValid( parameter ) ? AutoValidationControllerResult.Proceed : AutoValidationControllerResult.None;
			MarkValid( parameter, false );
			return status;
		}
	}
}
