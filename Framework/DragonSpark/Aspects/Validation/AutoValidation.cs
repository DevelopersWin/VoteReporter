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
	
	class AdapterLocator : FactoryBase<object, ParameterInstanceProfile?>
	{
		readonly ImmutableArray<IProfile> profiles;

		public static AdapterLocator Instance { get; } = new AdapterLocator();
		AdapterLocator() : this( AutoValidation.DefaultProfiles ) {}

		public AdapterLocator( ImmutableArray<IProfile> profiles )
		{
			this.profiles = profiles;
		}

		public override ParameterInstanceProfile? Create( object parameter )
		{
			var other = parameter.GetType();
			foreach ( var profile in profiles )
			{
				if ( profile.InterfaceType.IsAssignableFrom( other ) )
				{
					return profile.ProfileSource( parameter );
				}
			}
			return null;
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

	public class AutoValidationController : IAutoValidationController
	{
		readonly IParameterValidationAdapter validator;
		readonly IParameterValidationMonitor monitor;
		readonly IParameterAwareHandler handler;

		public AutoValidationController( IParameterValidationAdapter adapter, IParameterAwareHandler handler ) : this( adapter, handler, new ParameterValidationMonitor() ) {}

		public AutoValidationController( IParameterValidationAdapter validator, IParameterAwareHandler handler, IParameterValidationMonitor monitor )
		{
			this.validator = validator;
			this.handler = handler;
			this.monitor = monitor;
		}

		public bool? IsValid( object parameter ) => handler.Handles( parameter ) ? true : ( monitor.IsWatching( parameter ) ? monitor.IsValid( parameter ) : (bool?)null );

		public void MarkValid( object parameter, bool valid ) => monitor.MarkValid( parameter, valid );

		public AutoValidationControllerResult Execute( object parameter, out object result )
		{
			var status = 
				handler.Handle( parameter, out result ) ? AutoValidationControllerResult.ResultFound 
				: 
				monitor.IsValid( parameter ) || validator.IsValid( parameter ) ? AutoValidationControllerResult.Proceed : AutoValidationControllerResult.None;
			monitor.MarkValid( parameter, false );
			return status;
		}
	}
}
