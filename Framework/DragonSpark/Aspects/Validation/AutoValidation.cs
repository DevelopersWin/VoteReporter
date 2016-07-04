using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IProfile> DefaultProfiles { get; } = new IProfile[] { GenericFactoryProfile.Instance, FactoryProfile.Instance, GenericCommandProfile.Instance, CommandProfile.Instance }.ToImmutableArray();

		public static Func<object, IParameterValidationAdapter> DefaultAdapterSource { get; } = AdapterLocator.Instance.ToDelegate();

		class GenericFactoryProfile : Profile
		{
			public static GenericFactoryProfile Instance { get; } = new GenericFactoryProfile();
			GenericFactoryProfile() : base( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), GenericFactoryAdapterFactory.Instance ) {}
		}

		class FactoryProfile : Profile
		{
			public static FactoryProfile Instance { get; } = new FactoryProfile();
			FactoryProfile() : base( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), FactoryAdapterFactory.Instance ) {}
		}

		class GenericCommandProfile : Profile
		{
			public static GenericCommandProfile Instance { get; } = new GenericCommandProfile();
			GenericCommandProfile() : base( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute), GenericCommandAdapterFactory.Instance ) {}
		}

		class CommandProfile : Profile
		{
			public static CommandProfile Instance { get; } = new CommandProfile();
			CommandProfile() : base( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute), CommandAdapterFactory.Instance ) {}
		}
	}
	
	class AdapterLocator : FactoryBase<object, IParameterValidationAdapter>
	{
		readonly ImmutableArray<IProfile> profiles;

		public static AdapterLocator Instance { get; } = new AdapterLocator();
		AdapterLocator() : this( AutoValidation.DefaultProfiles ) {}

		public AdapterLocator( ImmutableArray<IProfile> profiles )
		{
			this.profiles = profiles;
		}

		public override IParameterValidationAdapter Create( object parameter )
		{
			var other = parameter.GetType();
			foreach ( var profile in profiles )
			{
				if ( profile.InterfaceType.IsAssignableFrom( other ) )
				{
					return profile.CreateAdapter( parameter );
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

	public class ParameterHandlerLocator : FactoryBase<IParameterValidationAdapter, IParameterAwareHandler>
	{
		public static ParameterHandlerLocator Instance { get; } = new ParameterHandlerLocator();

		readonly IDelegateParameterHandlerRegistry registry;

		ParameterHandlerLocator() : this( DelegateParameterHandlerRegistry.Instance ) {}

		public ParameterHandlerLocator( IDelegateParameterHandlerRegistry registry )
		{
			this.registry = registry;
		}

		public override IParameterAwareHandler Create( IParameterValidationAdapter parameter )
		{
			var factory = parameter.GetFactory();
			var handlers = factory != null ? registry.Get( factory ) : ImmutableArray<IParameterAwareHandler>.Empty;
			var result = handlers.ToArray().Only() ?? new CompositeParameterAwareHandler( handlers );
			return result;
		}
	}

	public enum AutoValidationControllerResult { None, ResultFound, Proceed }

	public class AutoValidationController : IAutoValidationController
	{
		readonly IParameterValidationAdapter validator;
		readonly IParameterValidationMonitor monitor;
		readonly IParameterAwareHandler handler;

		public AutoValidationController( IParameterValidationAdapter adapter ) : this( adapter, new ParameterValidationMonitor(), ParameterHandlerLocator.Instance.Create( adapter ) ) {}

		public AutoValidationController( IParameterValidationAdapter validator, IParameterValidationMonitor monitor, IParameterAwareHandler handler )
		{
			this.validator = validator;
			this.monitor = monitor;
			this.handler = handler;
		}

		public bool? IsValid( object parameter )
		{
			var handles = handler.Handles( parameter );
			return handles ? true : ( monitor.IsWatching( parameter ) ? monitor.IsValid( parameter ) : (bool?)null );
		}

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
