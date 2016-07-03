using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IProfile> DefaultProfiles { get; } = new IProfile[] { GenericFactoryProfile.Instance, FactoryProfile.Instance, /*GenericCommandProfile.Instance, CommandProfile.Instance*/ }.ToImmutableArray();

		public static Func<object, IAutoValidationController> Controller { get; } = new Cache<IAutoValidationController>( o => new AutoValidationController( AdapterLocator.Instance.Create( o ) ) ).ToDelegate();

		public static Func<object, IParameterValidationMonitor> Monitor { get; } = new Cache<IParameterValidationMonitor>( o => new ParameterValidationMonitor() ).ToDelegate();
		public static Func<object, IParameterValidationAdapter> Adapter { get; } = new Cache<IParameterValidationAdapter>( AdapterLocator.Instance.ToDelegate() ).ToDelegate();

		class FactoryProfile : Profile
		{
			public static FactoryProfile Instance { get; } = new FactoryProfile();
			FactoryProfile() : base( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), FactoryAdapterFactory.Instance ) {}
		}

		class GenericFactoryProfile : Profile
		{
			public static GenericFactoryProfile Instance { get; } = new GenericFactoryProfile();
			GenericFactoryProfile() : base( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), GenericFactoryAdapterFactory.Instance ) {}
		}
	}

	/*class FactoryProfile : AspectBuilderProfileBase<IFactoryWithParameter>
	{
		public static FactoryProfile Instance { get; } = new FactoryProfile();
		FactoryProfile() : base( Validation.Instance, Execution.Instance ) {}

		new class Validation : CommandProfileBase
		{
			public static Validation Instance { get; } = new Validation();
			Validation() : base( nameof(IFactoryWithParameter.CanCreate), AutoValidationCommandFactory.Instance ) {}
		}

		new class Execution : CommandProfileBase
		{
			public static Execution Instance { get; } = new Execution();
			Execution() : base( nameof(IFactoryWithParameter.Create), AutoValidationExecutionCommandFactory.Instance ) {}
		}
	}

	abstract class AspectBuilderProfileBase<T> : AspectBuilderProfile
	{
		protected AspectBuilderProfileBase( AspectCommandProfile validation, AspectCommandProfile execution ) : base( typeof(T), validation, execution ) {}

		public abstract class CommandProfileBase : AspectCommandProfile
		{
			protected CommandProfileBase( string methodName, Func<Delegate, IAspectCommand> factory ) : base( typeof(T), methodName, factory ) {}
		}
	}

	class AspectBuilderProfile
	{
		public AspectBuilderProfile( Type interfaceType, AspectCommandProfile validation, AspectCommandProfile execution )
		{
			InterfaceType = interfaceType;
			Validation = validation;
			Execution = execution;
		}

		public Type InterfaceType { get; }
		public AspectCommandProfile Validation { get; }
		public AspectCommandProfile Execution { get; }
	}

	class AspectCommandProfile
	{
		public AspectCommandProfile( Type interfaceType, string methodName, Func<Delegate, IAspectCommand> factory )
		{
			InterfaceType = interfaceType;
			MethodName = methodName;
			Factory = factory;
		}

		public Type InterfaceType { get; }
		public string MethodName { get; }
		public Func<Delegate, IAspectCommand> Factory { get; }
	}*/

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
			foreach ( var profile in profiles )
			{
				if ( profile.InterfaceType.IsInstanceOfTypeOrDefinition( parameter.GetType() ) )
				{
					return profile.AdapterFactory( parameter );
				}
			}
			return null;
		}
	}

	class AutoValidationCommandFactory : FactoryBase<Delegate, IAspectCommand<bool>>
	{
		public static Func<Delegate, IAspectCommand<bool>> Instance { get; } = new AutoValidationCommandFactory().ToDelegate();
		AutoValidationCommandFactory() : this( AutoValidation.Monitor ) {}

		readonly Func<object, IParameterValidationMonitor> monitor;

		public AutoValidationCommandFactory( Func<object, IParameterValidationMonitor> monitor )
		{
			this.monitor = monitor;
		}

		public override IAspectCommand<bool> Create( Delegate parameter ) => new AutoValidationValidationCommand( monitor( parameter.Target ) );
	}

	class AutoValidationExecutionCommandFactory : FactoryBase<Delegate, IAspectCommand<object>>
	{
		public static Func<Delegate, IAspectCommand<object>> Instance { get; } = new AutoValidationExecutionCommandFactory().ToDelegate();
		AutoValidationExecutionCommandFactory() : this( AutoValidation.Adapter, AutoValidation.Monitor ) {}

		readonly Func<object, IParameterValidationAdapter> adapterSource;
		readonly Func<object, IParameterValidationMonitor> monitorSource;
		
		public AutoValidationExecutionCommandFactory( Func<object, IParameterValidationAdapter> adapterSource, Func<object, IParameterValidationMonitor> monitorSource )
		{
			this.adapterSource = adapterSource;
			this.monitorSource = monitorSource;
		}

		public override IAspectCommand<object> Create( Delegate parameter ) => new AutoValidationExecutionCommand( adapterSource( parameter.Target ), monitorSource( parameter.Target ) );
	}

	/*class AdapterParameterHandler : IParameterAwareHandler
	{
		readonly IParameterValidationAdapter adapter;
		public AdapterParameterHandler( IParameterValidationAdapter adapter )
		{
			this.adapter = adapter;
		}

		public bool Handles( object parameter ) => adapter.IsValid( parameter );

		public bool Handle( object parameter, out object handled )
		{
			handled = null;
			return false;
		}
	}*/

	public interface IAutoValidationController
	{
		bool Validate( object parameter, Func<bool> proceed );

		object Execute( object parameter, Func<object> proceed );
	}

	public interface IAspectCommand {}

	public interface IAspectCommand<T> : IAspectCommand
	{
		T Execute( object parameter, Func<T> proceed );
	}

	public abstract class AspectCommandBase<T> : IAspectCommand<T>
	{
		public abstract T Execute( object parameter, Func<T> proceed );
	}

	public abstract class AutoValidationCommandBase<T> : AspectCommandBase<T>
	{
		protected AutoValidationCommandBase( IParameterValidationMonitor monitor )
		{
			Monitor = monitor;
		}

		protected IParameterValidationMonitor Monitor { get; }
	}

	public class AutoValidationValidationWithHandlerCommand : AutoValidationValidationCommand
	{
		readonly IParameterAwareHandler handler;
		public AutoValidationValidationWithHandlerCommand( IParameterAwareHandler handler, IParameterValidationMonitor monitor ) : base( monitor )
		{
			this.handler = handler;
		}

		public override bool Execute( object parameter, Func<bool> proceed ) => handler.Handles( parameter ) || base.Execute( parameter, proceed );
	}

	public class AutoValidationValidationCommand : AutoValidationCommandBase<bool>
	{
		public AutoValidationValidationCommand( IParameterValidationMonitor monitor ) : base( monitor ) {}

		public override bool Execute( object parameter, Func<bool> proceed ) => Monitor.Update( parameter, proceed );
	}

	public class AutoValidationExecutionWithHandlerCommand : AutoValidationExecutionCommand
	{
		readonly IParameterAwareHandler handler;
		public AutoValidationExecutionWithHandlerCommand( IParameterValidationAdapter adapter, IParameterAwareHandler handler, IParameterValidationMonitor monitor ) : base( adapter, monitor )
		{
			this.handler = handler;
		}

		public override object Execute( object parameter, Func<object> proceed )
		{
			object handled;
			var result = handler.Handle( parameter, out handled ) ? handled : base.Execute( parameter, proceed );
			return result;
		}
	}

	public class AutoValidationExecutionCommand : AutoValidationCommandBase<object>
	{
		readonly Func<object, bool> validation;

		public AutoValidationExecutionCommand( IParameterValidationAdapter adapter, IParameterValidationMonitor monitor ) : this( monitor, adapter.IsValid ) {}

		AutoValidationExecutionCommand( IParameterValidationMonitor monitor, Func<object, bool> validation ) : base( monitor )
		{
			this.validation = validation;
		}

		public override object Execute( object parameter, Func<object> proceed )
		{
			var result = Monitor.Update( parameter, validation ) ? proceed() : null;
			Monitor.Clear( parameter );
			return result;
		}
	}

	public class AutoValidationController : IAutoValidationController
	{
		readonly Func<object, bool> validator;
		readonly IParameterValidationMonitor monitor;

		public AutoValidationController( IParameterValidationAdapter adapter ) : this( adapter.IsValid, new ParameterValidationMonitor() ) {}

		public AutoValidationController( Func<object, bool> validator, IParameterValidationMonitor monitor )
		{
			this.validator = validator;
			this.monitor = monitor;
		}

		public bool Validate( object parameter, Func<bool> proceed ) => monitor.Update( parameter, proceed );
		
		public object Execute( object parameter, Func<object> proceed )
		{
			var result = monitor.Update( parameter, validator ) ? proceed() : null;
			monitor.Clear( parameter );
			return result;
		}
	}
}
