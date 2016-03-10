using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	public class ExportDescriptorProvider : System.Composition.Hosting.Core.ExportDescriptorProvider
	{
		readonly IUnityContainer container;
		readonly AccessMonitor<IEnumerable<ExportDescriptorPromise>> monitor = new AccessMonitor<IEnumerable<ExportDescriptorPromise>>( Default<ExportDescriptorPromise>.Items );

		public ExportDescriptorProvider( [Required]IUnityContainer container )
		{
			// Current = current;
			this.container = container;
		}

		// public IWritableValue<NamedTypeBuildKey> Current { get; }

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor ) //=> monitor.Access( () =>
		{
			CompositionDependency dependency;
			// var enabled = Current.Item == null || new NamedTypeBuildKey( contract.ContractType, contract.ContractName ) != Current.Item;
			var existing = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			var result = // enabled && 
				!existing && container.IsRegistered( contract.ContractType, contract.ContractName )
					? new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => container.Resolve( contract.ContractType, contract.ContractName ), NoMetadata ) ).ToItem()
					: monitor.DefaultValue;
			return result;
		}// );
	}

	public abstract class MonitorExtensionBase : UnityContainerExtension, IDisposable
	{
		protected override void Initialize() => Context.RegisteringInstance += OnRegisteringInstance;

		protected abstract void OnRegisteringInstance( object sender, RegisterInstanceEventArgs args );

		public override void Remove()
		{
			base.Remove();

			Context.RegisteringInstance -= OnRegisteringInstance;
		}

		public void Dispose() => Remove();
	}

	public class DefaultInjection : InjectionMember
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
	}

	public class RegisterDefaultCommand : Command<RegisterDefaultCommand.Parameter>
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

			public void Assign( T item ) => base.Instance = item;
		}

		public class Parameter
		{
			public Parameter( [Required]Type type, [Required]object instance )
			{
				Type = type;
				Instance = instance;
				new DefaultRegistrationsExtension.Default( Instance ).Assign( true );
			}

			public Type Type { get; }
			public object Instance { get; protected set; }
		}

		protected override void OnExecute( Parameter parameter ) => 
			new[] { parameter.Type, parameter.Instance.GetType() }.Distinct().WhereNot( container.IsRegistered ).Each( type =>
			{
				var factory = new InjectionFactory( c => parameter.Instance );
				var member = new DefaultInjection( factory );
				container.RegisterType( type, member );
			} );
	}

	class MonitorLoggerRegistrationCommand : MonitorRegistrationCommandBase<ILogger>
	{
		readonly RecordingLogEventSink sink;

		public MonitorLoggerRegistrationCommand( [Required]RecordingLogEventSink sink, RegisterDefaultCommand.Parameter<ILogger> parameter ) : base( parameter )
		{
			this.sink = sink;
		}

		protected override void OnExecute( ILogger parameter )
		{
			var messages = sink.Purge();
			parameter.Information( $"A new logger of type '{parameter}' has been registered.  Purging existing logger with '{messages.Length}' messages and routing them through the new logger." );
			messages.Each( parameter.Write );
			base.OnExecute( parameter );
		}
	}

	class MonitorRecordingSinkRegistrationCommand : MonitorRegistrationCommandBase<RecordingLogEventSink>
	{
		public MonitorRecordingSinkRegistrationCommand( RegisterDefaultCommand.Parameter<RecordingLogEventSink> parameter ) : base( parameter ) {}

		protected override void OnExecute( RecordingLogEventSink parameter )
		{
			Parameter.Instance().Events.Each( parameter.Emit );
			base.OnExecute( parameter );
		}
	}

	abstract class MonitorRegistrationCommandBase<T> : Command<T, ISpecification<T>> where T : class
	{
		protected MonitorRegistrationCommandBase( [Required] RegisterDefaultCommand.Parameter<T> parameter ) : base( new WrappedSpecification<T>( new AllSpecification( NullSpecification.NotNull, new OnlyOnceSpecification() ) ) )
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
	}

	public class DefaultRegistrationsExtension : MonitorExtensionBase
	{
		readonly RegisterDefaultCommand register;
		readonly PersistentServiceRegistry registry;
		readonly RegisterDefaultCommand.Parameter[] parameters;
		readonly ICollection<ICommand> commands;

		public DefaultRegistrationsExtension( [Required]IUnityContainer container, [Required]Assembly[] assemblies, [Required]RegisterDefaultCommand register, [Required]RecordingLogEventSink sink ) 
			: this( container, assemblies, register, sink, new RecordingLoggerFactory( sink ).Create() ) {}

		DefaultRegistrationsExtension( [Required] IUnityContainer container, [Required]Assembly[] assemblies, [Required] RegisterDefaultCommand register, [Required] RecordingLogEventSink sink, ILogger logger ) 
			: this( assemblies, register, sink, logger, new PersistentServiceRegistry( container, logger, new LifetimeManagerFactory<ContainerControlledLifetimeManager>( container) ) ) {}

		DefaultRegistrationsExtension( [Required]Assembly[] assemblies, [Required]RegisterDefaultCommand register, [Required]RecordingLogEventSink sink, [Required]ILogger logger, [Required]PersistentServiceRegistry registry )
		{
			this.register = register;
			this.registry = registry;

			var instance = Assemblies.Resolve( assemblies );
			var loggingParameter = new RegisterDefaultCommand.Parameter<ILogger>( logger );
			var sinkParameter = new RegisterDefaultCommand.Parameter<RecordingLogEventSink>( sink );
			parameters = new RegisterDefaultCommand.Parameter[]
			{
				new RegisterDefaultCommand.Parameter<Assembly[]>( instance ),
				sinkParameter,
				loggingParameter
			};
			commands = new Collection<ICommand>( new ICommand[] { new MonitorLoggerRegistrationCommand( sink, loggingParameter ), new MonitorRecordingSinkRegistrationCommand( sinkParameter ) } );
		}

		public class Default : AssociatedValue<object, bool>
		{
			public Default( object instance ) : base( instance ) {}
		}

		protected override void Initialize()
		{
			base.Initialize();

			parameters.Each( register.ExecuteWith );

			registry.Register<IServiceRegistry, ServiceRegistry>();
			registry.Register<IActivator, Activator>();
			registry.Register( Context );
			registry.Register( new Activation.Activator.Get( Activation.Activator.GetCurrent ) );
			registry.Register( new Assemblies.Get( Assemblies.GetCurrent ) );
		}

		class Activator : CompositeActivator
		{
			public Activator( [Required]IoC.Activator activator ) : base( activator, SystemActivator.Instance ) {}
		}

		protected override void OnRegisteringInstance( object sender, RegisterInstanceEventArgs args ) => commands.ToArray().Select( c => c.ExecuteWith( args.Instance ) ).NotNull().Each( commands.Remove );

		public override void Remove()
		{
			base.Remove();
			commands.Clear();
		}
	}

	public class InstanceTypeRegistrationMonitorExtension : MonitorExtensionBase
	{
		protected override void OnRegisteringInstance( object sender, RegisterInstanceEventArgs args )
		{
			var type = args.Instance.GetType();

			if ( args.RegisteredType != type && !Container.IsRegistered( type, args.Name ) )
			{
				var registry = new ServiceRegistry( Container, args.LifetimeManager.GetType() );
				registry.Register( new InstanceRegistrationParameter( type, args.Instance, args.Name ) );
			}
		}
	}

	public class BuildPipelineExtension : UnityContainerExtension
	{
		readonly MetadataLifetimeStrategy metadataLifetimeStrategy;
		readonly ConventionStrategy conventionStrategy;

		/*class BuildKeyMonitorStrategy : BuilderStrategy
		{
			readonly IList<NamedTypeBuildKey> keys = new List<NamedTypeBuildKey>();

			public IEnumerable<NamedTypeBuildKey> Purge() => keys.Purge();

			public override void PreBuildUp( IBuilderContext context ) => keys.Ensure( context.BuildKey );
		}*/

		public BuildPipelineExtension( [Required] MetadataLifetimeStrategy metadataLifetimeStrategy, [Required] ConventionStrategy conventionStrategy )
		{
			this.metadataLifetimeStrategy = metadataLifetimeStrategy;
			this.conventionStrategy = conventionStrategy;
		}

		protected override void Initialize()
		{
			/*var strategy = new BuildKeyMonitorStrategy();
			Context.BuildPlanStrategies.Add( strategy, UnityBuildStage.Setup );			

			strategy.Purge().Each( Context.Policies.ClearBuildPlan );

			Context.BuildPlanStrategies.Clear();
			Context.BuildPlanStrategies.AddNew<DynamicMethodConstructorStrategy>( UnityBuildStage.Creation );
			Context.BuildPlanStrategies.AddNew<DynamicMethodPropertySetterStrategy>( UnityBuildStage.Initialization );
			Context.BuildPlanStrategies.AddNew<DynamicMethodCallStrategy>( UnityBuildStage.Initialization );*/

			Context.Strategies.Clear();
			Context.Strategies.AddNew<BuildKeyMappingStrategy>( UnityBuildStage.TypeMapping );
			Context.Strategies.Add( metadataLifetimeStrategy, UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<HierarchicalLifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<LifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.Add( conventionStrategy, UnityBuildStage.PreCreation );
			Context.Strategies.AddNew<ArrayResolutionStrategy>( UnityBuildStage.Creation );
			Context.Strategies.AddNew<EnumerableResolutionStrategy>( UnityBuildStage.Creation );
			Context.Strategies.AddNew<BuildPlanStrategy>( UnityBuildStage.Creation );

			var policy = Context.Policies.Get<IBuildPlanCreatorPolicy>( null );
			var builder = new Builder<TryContext>( Context.Strategies, policy.CreatePlan );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( new BuildPlanCreatorPolicy( builder.Create, Policies, policy ) );
			Context.Policies.SetDefault<IConstructorSelectorPolicy>( DefaultUnityConstructorSelectorPolicy.Instance );
		}

		public class MetadataLifetimeStrategy : BuilderStrategy
		{
			readonly Func<ILogger> logger;
			readonly LifetimeManagerFactory factory;

			public MetadataLifetimeStrategy( [Required]Func<ILogger> logger, [Required]LifetimeManagerFactory factory )
			{
				this.logger = logger;
				this.factory = factory;
			}

			public override void PreBuildUp( IBuilderContext context )
			{
				var reference = new KeyReference( this, context.BuildKey ).Item;
				if ( new Checked( reference, this ).Item.Apply() )
				{
					var lifetimePolicy = context.Policies.GetNoDefault<ILifetimePolicy>( context.BuildKey, false );
					lifetimePolicy.Null( () =>
					{
						var lifetimeManager = factory.Create( reference.Type );
						lifetimeManager.With( manager =>
						{
							logger().Debug( $"'{GetType().Name}' is assigning a lifetime manager of '{manager.GetType()}' for type '{reference.Type}'." );

							context.PersistentPolicies.Set<ILifetimePolicy>( manager, reference );
						} );
					} );
				}
			}
		}

		public class Builder<T> : FactoryBase<IBuilderContext, T>
		{
			readonly NamedTypeBuildKey key = NamedTypeBuildKey.Make<T>();
			readonly IStagedStrategyChain strategies;
			readonly Func<IBuilderContext, NamedTypeBuildKey, IBuildPlanPolicy> creator;

			public Builder( [Required]IStagedStrategyChain strategies, [Required]Func<IBuilderContext, NamedTypeBuildKey, IBuildPlanPolicy> creator )
			{
				this.strategies = strategies;
				this.creator = creator;
			}

			protected override T CreateItem( IBuilderContext parameter )
			{
				var context = new BuilderContext( strategies.MakeStrategyChain(), parameter.Lifetime, parameter.PersistentPolicies, parameter.Policies, key, null );
				var plan = creator( context, key );
				plan.BuildUp( context );
				var result = context.Existing.To<T>();
				return result;
			}
		}

		public IList<IBuildPlanPolicy> Policies { get; } = new List<IBuildPlanPolicy> { new SingletonBuildPlanPolicy() };
	}

	public class CompositionExtension : UnityContainerExtension
	{
		readonly ComposeStrategy strategy;
		readonly CompositionHost host;
		readonly ExportDescriptorProvider provider;
		readonly RegisterHierarchyCommand command;

		public CompositionExtension( [Required]IServiceRegistry registry, [Required]Factory factory, ExportDescriptorProvider provider, PersistentServiceRegistry persistentServiceRegistry ) 
			: this( registry, factory.Create(), provider, new RegisterHierarchyCommand( persistentServiceRegistry ) ) {}

		CompositionExtension( IServiceRegistry registry, CompositionHost host, ExportDescriptorProvider provider, RegisterHierarchyCommand command )
			: this( new ComposeStrategy( host, registry ), host, provider, command ) {}

		CompositionExtension( [Required]ComposeStrategy strategy, [Required]CompositionHost host, [Required]ExportDescriptorProvider provider, [Required]RegisterHierarchyCommand command )
		{
			this.strategy = strategy;
			this.host = host;
			this.provider = provider;
			this.command = command;
		}

		protected override void Initialize()
		{
			command.ExecuteWith( new InstanceRegistrationParameter( host ) );

			host.GetExport<IExportDescriptorProviderRegistry>().Register( provider );

			Context.Strategies.Add( strategy, UnityBuildStage.PreCreation );
		}

		public class Factory : FirstFactory<CompositionHost>
		{
			public Factory( [Required]IActivator activator, [Required]Assembly[] assemblies ) : base( activator.Activate<CompositionHost>, () => Composer.Current, () => CompositionHostFactory.Instance.Create( assemblies ) ) {}
		}
	}

	public class BuildableTypeFromConventionLocator : FactoryBase<Type, Type>
	{
		readonly Assembly[] assemblies;
		readonly CanBuildSpecification specification;

		public BuildableTypeFromConventionLocator( [Required]Assembly[] assemblies ) : this( assemblies, CanBuildSpecification.Instance ) {}

		public BuildableTypeFromConventionLocator( [Required]Assembly[] assemblies, [Required]CanBuildSpecification specification )
		{
			this.assemblies = assemblies;
			this.specification = specification;
		}

		protected override Type CreateItem( Type parameter )
		{
			var adapter = parameter.Adapt();
			var name = parameter.Name.TrimStartOf( 'I' );
			var result = assemblies.Append( parameter.Assembly() ).Distinct()
				.SelectMany( assembly => assembly.DefinedTypes.AsTypes() )
				.Where( adapter.IsAssignableFrom )
				.Where( specification.IsSatisfiedBy )
				.FirstOrDefault( candidate => candidate != parameter && candidate.Name.StartsWith( name ) );
			return result;
		}
	}

	public class ImplementedFromConventionTypeLocator : FactoryBase<Type, Type>
	{
		public static ImplementedFromConventionTypeLocator Instance { get; } = new ImplementedFromConventionTypeLocator();

		[Freeze]
		protected override Type CreateItem( Type parameter )
		{
			var assemblies = new Assemblies.Get[] { Assemblies.GetCurrent, parameter.Append( GetType() ).Distinct().Assemblies };

			var result = assemblies.FirstWhere( get => new ImplementedInterfaceFromConventionLocator( get() ).Create( parameter ) );
			return result;
		}
	}

	public class ImplementedInterfaceFromConventionLocator : FactoryBase<Type,Type>
	{
		readonly Assembly[] assemblies;

		public ImplementedInterfaceFromConventionLocator( [Required]Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}

		protected override Type CreateItem( Type parameter )
		{
			var result =
				parameter.GetTypeInfo().ImplementedInterfaces.ToArray().With( interfaces => 
					interfaces.FirstOrDefault( i => parameter.Name.Contains( i.Name.TrimStartOf( 'I' ) ) )
					??
					interfaces.FirstOrDefault( t => assemblies.Contains( t.Assembly() ) )
				) ?? parameter;
			return result;
		}
	}

	public class CanBuildSpecification : SpecificationBase<Type>
	{
		public static CanBuildSpecification Instance { get; } = new CanBuildSpecification();

		[Freeze]
		protected override bool Verify( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = parameter != typeof(object) && !info.IsInterface && !info.IsAbstract && !typeof(Delegate).Adapt().IsAssignableFrom( parameter ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class InvalidBuildFromContextSpecification : SpecificationBase<IBuilderContext>
	{
		public static InvalidBuildFromContextSpecification Instance { get; } = new InvalidBuildFromContextSpecification();

		readonly CanBuildSpecification specification;

		public InvalidBuildFromContextSpecification() : this( CanBuildSpecification.Instance ) {}

		public InvalidBuildFromContextSpecification( [Required]CanBuildSpecification specification )
		{
			this.specification = specification;
		}

		protected override bool Verify( IBuilderContext parameter ) => !specification.IsSatisfiedBy( parameter.BuildKey.Type ) || !CanBuildFrom( parameter );

		static bool CanBuildFrom( IBuilderContext parameter )
		{
			IPolicyList containingPolicyList;
			var constructor = parameter.Policies.Get<IConstructorSelectorPolicy>( parameter.BuildKey, out containingPolicyList).SelectConstructor(parameter, containingPolicyList);
			var result = constructor.With( IsValidConstructor );
			return result;
		}

		static bool IsValidConstructor( SelectedConstructor selectedConstructor ) => selectedConstructor.Constructor.GetParameters().All( pi => !pi.ParameterType.IsByRef );
	}

	class KeyReference : Reference<NamedTypeBuildKey>
	{
		public KeyReference( object instance, NamedTypeBuildKey key ) : base( instance, key ) { }
	}

	/*public class DeferredExecutionMonitor
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		readonly ICollection<Action> delegates = new System.Collections.ObjectModel.Collection<Action>();

		public void Execute( [Required]Action action )
		{
			if ( monitor.IsApplied )
			{
				action();
			}
			else
			{
				delegates.Add( action );
			}
		}

		public void Apply() => monitor.Apply( () => delegates.Purge().Each( action => action() ) );
	}*/

	public class ComposeStrategy : BuilderStrategy
	{
		readonly IServiceRegistry registry;
		readonly CompositionHost host;

		public ComposeStrategy( [Required] CompositionHost host, [Required]IServiceRegistry registry )
		{
			this.registry = registry;
			this.host = host;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = new KeyReference( this, context.BuildKey ).Item;
			object existing = null;
			var process = !context.HasBuildPlan() && new Checked( reference, this ).Item.Apply() && host.TryGetExport( context.BuildKey.Type, context.BuildKey.Name, out existing ) && !new DefaultRegistrationsExtension.Default( existing ).Item;
			process.IsTrue( () =>
			{
				registry.Register( new InstanceRegistrationParameter( context.BuildKey.Type, existing, context.BuildKey.Name ) );

				context.Complete( existing );
			} );
		}
	}

	public class ConventionStrategy : BuilderStrategy
	{
		readonly ConventionCandidateLocator locator;
		readonly IServiceRegistry registry;

		// [Persistent]
		public class ConventionCandidateLocator : SpecificationAwareFactory<IBuilderContext, Type>
		{
			public ConventionCandidateLocator( [Required]BuildableTypeFromConventionLocator factory ) : this( InvalidBuildFromContextSpecification.Instance, factory ) { }

			ConventionCandidateLocator( [Required]InvalidBuildFromContextSpecification specification, [Required]BuildableTypeFromConventionLocator factory ) : base( specification, context => factory.Create( context.BuildKey.Type ) ) { }
		}

		public ConventionStrategy( [Required]ConventionCandidateLocator locator, [Required]IServiceRegistry registry )
		{
			this.locator = locator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = new KeyReference( this, context.BuildKey ).Item;
			if ( new Checked( reference, this ).Item.Apply() )
			{
				var convention = locator.Create( context );
				convention.With( located =>
				{
					var from = context.BuildKey.Type;
					context.BuildKey = new NamedTypeBuildKey( located, context.BuildKey.Name );
					
					registry.Register( new MappingRegistrationParameter( from, context.BuildKey.Type, context.BuildKey.Name ) );
				} );
			}
		}
	}
}