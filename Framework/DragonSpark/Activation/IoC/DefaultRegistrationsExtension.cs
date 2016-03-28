using DragonSpark.Setup.Registration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Activation.IoC
{
	public class DefaultRegistrationsExtension : UnityContainerExtension
	{
		readonly PersistentServiceRegistry registry;

		public DefaultRegistrationsExtension( [Required]PersistentServiceRegistry registry )
		{
			this.registry = registry;

			/*var loggingParameter = new RegisterDefaultCommand.Parameter<ILogger>( logger );
			var sinkParameter = new RegisterDefaultCommand.Parameter<ILoggerHistory>( history );
			parameters = new RegisterDefaultCommand.Parameter[]
			{
				new RegisterDefaultCommand.Parameter<Assembly[]>( assemblies ),
				new RegisterDefaultCommand.Parameter<Type[]>( TypesFactory.Instance.Create( assemblies ) ),
				new RegisterDefaultCommand.Parameter<Serilog.Core.LoggingLevelSwitch>( levelSwitch ),
				sinkParameter,
				loggingParameter
			};
			commands = new Collection<ICommand>( new ICommand[] { new MonitorLoggerRegistrationCommand( history, loggingParameter ), new MonitorLoggerHistoryRegistrationCommand( sinkParameter ) } );*/
		}

		protected override void Initialize()
		{
			// base.Initialize();

			// parameters.Each( register.ExecuteWith );

			registry.Register( Context );
			registry.Register( Context.Policies );
			registry.Register<IStagedStrategyChain>( Context.BuildPlanStrategies );

			registry.Register<IServiceRegistry, ServiceRegistry>();
			registry.Register<IActivator, Activator>();
		}

		class Activator : CompositeActivator
		{
			public Activator( [Required]Locator locator, [Required]Constructor constructor ) : base( locator, constructor, Activation.Constructor.Instance ) {}
		}

		/*protected override void OnRegisteringInstance( object sender, RegisterInstanceEventArgs args ) => commands.ExecuteMany( args.Instance ).NotNull().ToArray().Each( command =>
		{
			if ( commands.Remove( command ) )
			{
				new RegisterDefaultCommand.Default( new KeyReference( Container, new NamedTypeBuildKey( args.RegisteredType ) ).Item ).Assign( false );
			}
		} );

		public override void Remove()
		{
			base.Remove();
			commands.Clear();
		}*/
	}

	/*public class RegisterDefaultCommand : Command<RegisterDefaultCommand.Parameter>
	{
		readonly IUnityContainer container;

		public RegisterDefaultCommand( [Required]IUnityContainer container )
		{
			this.container = container;
		}

		public class Parameter<T> : Parameter
		{
			public Parameter( T instance ) : base( typeof(T), instance ) {}

			public new T Instance() => (T)base.Instance;

			public void Assign( T item )
			{
				new Default( base.Instance ).Assign( false );
				base.Instance = item;
			}
		}

		public class Parameter
		{
			public Parameter( [Required]System.Type type, [Required]object instance )
			{
				Type = type;
				Instance = instance;
			}

			public System.Type Type { get; }
			public object Instance { get; protected set; }
		}

		protected override void OnExecute( Parameter parameter ) => 
			new[] { parameter.Type, parameter.Instance.GetType() }.Distinct().WhereNot( container.IsRegistered ).Each( type =>
			{
				var factory = new InjectionFactory( c => parameter.Instance );
				var member = new DefaultInjection( factory );
				container.RegisterType( type, member );
				new[] { new KeyReference( container, new NamedTypeBuildKey( type ) ).Item, parameter.Instance }.Each( o =>
				{
					new Default( o ).Assign( true );
				} );
			} );

		public class Default : AssociatedValue<object, bool>
		{
			public Default( object instance ) : base( instance ) {}
		}
	}*/

	/*class MonitorLoggerRegistrationCommand : MonitorRegistrationCommandBase<ILogger>
	{
		readonly ILoggerHistory history;

		public MonitorLoggerRegistrationCommand( [Required]ILoggerHistory history, RegisterDefaultCommand.Parameter<ILogger> parameter ) : base( parameter )
		{
			this.history = history;
		}

		protected override void OnExecute( ILogger parameter )
		{
			parameter.Information( "A new logger of type {Type} has been registered.  Purging existing logger with {Messages} messages and routing them through the new logger.", 
				parameter.GetType(),
				history.Events.Count()
				);
			new PurgeLoggerHistoryCommand( history ).ExecuteWith( new Action<LogEvent>( parameter.Write ) );
			base.OnExecute( parameter );
		}
	}

	class MonitorLoggerHistoryRegistrationCommand : MonitorRegistrationCommandBase<ILoggerHistory>
	{
		public MonitorLoggerHistoryRegistrationCommand( RegisterDefaultCommand.Parameter<ILoggerHistory> parameter ) : base( parameter ) {}

		protected override void OnExecute( ILoggerHistory parameter )
		{
			Parameter.Instance().Events.Each( parameter.Emit );
			base.OnExecute( parameter );
		}
	}

	abstract class MonitorRegistrationCommandBase<T> : Command<T, ISpecification<T>> where T : class
	{
		protected MonitorRegistrationCommandBase( [Required] RegisterDefaultCommand.Parameter<T> parameter ) : base( new DecoratedSpecification<T>( new AllSpecification( NullSpecification.NotNull, new OnlyOnceSpecification() ) ) )
		{
			Parameter = parameter;
		}

		protected RegisterDefaultCommand.Parameter<T> Parameter { get; }

		protected override void OnExecute( T parameter ) => Parameter.Assign( parameter );

		public override bool CanExecute( T parameter )
		{
			var result = parameter != Parameter.Instance() && base.CanExecute( parameter );
			return result;
		}
	}*/

	/*public class DefaultInjection : InjectionMember
	{
		readonly InjectionMember inner;

		public DefaultInjection( [Required]InjectionMember inner )
		{
			this.inner = inner;
		}

		public class Applied : Checked
		{
			public Applied( IBuildPlanPolicy instance ) : base( instance, typeof(Applied) ) {}
		}

		public override void AddPolicies( Type serviceType, Type implementationType, string name, IPolicyList policies )
		{
			var key = new NamedTypeBuildKey( implementationType, name );
			var before = policies.HasBuildPlan( key );
			if ( !before )
			{
				inner.AddPolicies( serviceType, implementationType, name, policies );
				policies.GetBuildPlan( key ).With( policy => new Applied( policy ).Item.Apply() );
			}
		}
	}*/

	/*public class DefaultValueStrategy : BuilderStrategy
	{
		readonly IUnityContainer container;
		readonly Func<ILogger> logger;

		public DefaultValueStrategy( [Required]IUnityContainer container, [Required]Func<ILogger> logger )
		{
			this.container = container;
			this.logger = logger;
		}

		public override void PostBuildUp( IBuilderContext context ) => context.Existing.With( existing =>
		{
			var reference = new KeyReference( container, context.BuildKey ).Item;
			var @default = new RegisterDefaultCommand.Default( reference );
			var isDefault = new RegisterDefaultCommand.Default( existing ).Item;
			if ( @default.Item && !isDefault )
			{
				@default.Assign( false );
				var message = $"'{GetType().Name}' is clearing the default registration of '{context.BuildKey.Type}'.";
				logger().Debug( message );

				context.Policies.ClearBuildPlan( reference );
			}
		} );
	}*/
}