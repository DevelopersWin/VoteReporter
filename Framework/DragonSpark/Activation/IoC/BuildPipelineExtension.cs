using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
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
using System.Composition;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Serilog.Core;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	public class ExportDescriptorProvider : System.Composition.Hosting.Core.ExportDescriptorProvider
	{
		readonly IUnityContainer container;

		public ExportDescriptorProvider( [Required]IUnityContainer container )
		{
			this.container = container;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) && container.IsRegistered( contract.ContractType, contract.ContractName ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => container.Resolve( contract.ContractType, contract.ContractName ), NoMetadata ) );
			}
		}
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
			public Parameter( object instance ) : base( typeof(T), instance ) {}
		}

		public class Parameter
		{
			public Parameter( [Required]Type type, [Required]object instance )
			{
				Type = type;
				Instance = instance;
			}

			public Type Type { get; }
			public object Instance { get; }
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

		public MonitorLoggerRegistrationCommand( [Required]RecordingLogEventSink sink, ILogger instance ) : base( instance )
		{
			this.sink = sink;
		}

		protected override void OnExecute( ILogger parameter )
		{
			var messages = sink.Purge();
			parameter.Information( $"A new logger of type '{parameter}' has been registered.  Purging existing logger with '{messages.Length}' messages and routing them through the new logger." );
			messages.Each( parameter.Write );
		}
	}

	class MonitorRecordingSinkRegistrationCommand : MonitorRegistrationCommandBase<RecordingLogEventSink>
	{
		public MonitorRecordingSinkRegistrationCommand( RecordingLogEventSink instance ) : base( instance ) {}

		protected override void OnExecute( RecordingLogEventSink parameter ) => Instance.Events.Each( parameter.Emit );
	}

	abstract class MonitorRegistrationCommandBase<T> : Command<T, ISpecification<T>> where T : class
	{
		protected MonitorRegistrationCommandBase( [Required]T instance ) : base( new WrappedSpecification<T>( new AllSpecification( NullSpecification.NotNull, new OnlyOnceSpecification() ) ) )
		{
			Instance = instance;
		}

		protected T Instance { get; }

		public override bool CanExecute( T parameter )
		{
			var result = parameter != Instance && base.CanExecute( parameter );
			return result;
		}
	}

	public class DefaultRegistrationsExtension : MonitorExtensionBase
	{
		readonly ICollection<ICommand> commands = new Collection<ICommand>();

		protected override void Initialize()
		{
			base.Initialize();

			var command = new RegisterDefaultCommand( Container );
			
			var sink = new RecordingLogEventSink();
			var logger = new RecordingLoggerFactory( sink ).Create();

			var parameters = new RegisterDefaultCommand.Parameter[]
			{
				new RegisterDefaultCommand.Parameter<ILogEventSink>( sink ),
				new RegisterDefaultCommand.Parameter<ILogger>( logger )
			};

			parameters.Each( command.ExecuteWith );

			commands.AddRange( new ICommand[] { new MonitorLoggerRegistrationCommand( sink, logger ), new MonitorRecordingSinkRegistrationCommand( sink ) } );
		}

		protected override void OnRegisteringInstance( object sender, RegisterInstanceEventArgs args ) => commands.ToArray().Select( command => command.ExecuteWith( args.Instance ) ).NotNull().Each( x => commands.Remove( x ) );

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
		class BuildKeyMonitorStrategy : BuilderStrategy
		{
			readonly IList<NamedTypeBuildKey> keys = new List<NamedTypeBuildKey>();

			public IEnumerable<NamedTypeBuildKey> Purge() => keys.Purge();

			public override void PreBuildUp( IBuilderContext context ) => keys.Ensure( context.BuildKey );
		}

		class MetadataLifetimeStrategy : BuilderStrategy
		{
			[Required, Locate]
			ILogger Logger { get; set; }

			public override void PreBuildUp( IBuilderContext context )
			{
				var reference = new KeyReference( this, context.BuildKey ).Item;
				if ( new Checked( reference, this ).Item.Apply() )
				{
					var lifetimePolicy = context.Policies.GetNoDefault<ILifetimePolicy>( context.BuildKey, false );
					lifetimePolicy.Null( () =>
					{
						var factory = context.New<LifetimeManagerFactory>();
						var lifetimeManager = factory.Create( reference.Type );
						lifetimeManager.With( manager =>
						{
							var logger = Logger;
							logger.Information( $"'{GetType().Name}' is assigning a lifetime manager of '{manager.GetType()}' for type '{reference.Type}'." );

							context.PersistentPolicies.Set<ILifetimePolicy>( manager, reference );
						} );
					} );
				}
			}
		}

		[Required, Factory]
		public CompositionHost Host { [return: Required]get; set; }

		[Export]
		class CompositionHostFactory : FirstFactory<CompositionHost>
		{
			public CompositionHostFactory() : this( new CompositionHostContext(), new AssemblyHost() ) {}

			public CompositionHostFactory( [Required]CompositionHostContext context, [Required]AssemblyHost assemblies ) : base( () => context.Item, () => new Composition.CompositionHostFactory().Create( assemblies.Item ?? Default<Assembly>.Items ) ) {}
		}

		protected override void Initialize()
		{
			var monitor = new DeferredExecutionMonitor();
			Context.Policies.SetDefault<IConstructorSelectorPolicy>( DefaultUnityConstructorSelectorPolicy.Instance );
			Host.GetExport<IExportDescriptorProviderRegistry>().Register( new ExportDescriptorProvider( Container ) );

			Context.Strategies.Clear();
			Context.Strategies.AddNew<BuildKeyMappingStrategy>( UnityBuildStage.TypeMapping );
			Context.Strategies.AddNew<MetadataLifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<HierarchicalLifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<LifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<ConventionStrategy>( UnityBuildStage.PreCreation );
			Context.Strategies.Add( new ComposeStrategy( monitor ), UnityBuildStage.PreCreation );
			Context.Strategies.AddNew<ArrayResolutionStrategy>( UnityBuildStage.Creation );
			Context.Strategies.AddNew<EnumerableResolutionStrategy>( UnityBuildStage.Creation );
			Context.Strategies.AddNew<BuildPlanStrategy>( UnityBuildStage.Creation );

			var strategy = new BuildKeyMonitorStrategy();
			Context.BuildPlanStrategies.Add( strategy, UnityBuildStage.Setup );

			Context.New<PersistentServiceRegistry>().With( registry =>
			{
				registry.Register<IServiceRegistry, ServiceRegistry>();
				registry.Register<IActivator, Activator>();
				registry.Register( Context );
				registry.Register( new Activation.Activator.Get( Activation.Activator.GetCurrent ) );
				registry.Register( new Assemblies.Get( Assemblies.GetCurrent ) );

				new RegisterHierarchyCommand( registry ).ExecuteWith( new InstanceRegistrationParameter( Host ) );
			} );

			monitor.Apply();
			strategy.Purge().Each( Context.Policies.ClearBuildPlan );

			Context.BuildPlanStrategies.Clear();
			Context.BuildPlanStrategies.AddNew<DynamicMethodConstructorStrategy>( UnityBuildStage.Creation );
			Context.BuildPlanStrategies.AddNew<DynamicMethodPropertySetterStrategy>( UnityBuildStage.Initialization );
			Context.BuildPlanStrategies.AddNew<DynamicMethodCallStrategy>( UnityBuildStage.Initialization );

			var policy = Context.Policies.Get<IBuildPlanCreatorPolicy>( null );
			var builder = new Builder<TryContext>( Context.Strategies, policy.CreatePlan );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( new BuildPlanCreatorPolicy( builder.Create, Policies, policy ) );
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

		class Activator : CompositeActivator
		{
			public Activator( [Required]IoC.Activator activator ) : base( activator, SystemActivator.Instance ) {}
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
			var result = assemblies.AnyOr( () => parameter.Assembly().ToItem() )
				.SelectMany( assembly => assembly.DefinedTypes.AsTypes() )
				.Where( adapter.IsAssignableFrom )
				.Where( specification.IsSatisfiedBy )
				.FirstOrDefault( candidate => candidate.Name.StartsWith( name ) );
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

		protected override bool Verify( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = parameter != typeof(object) && !info.IsInterface && !info.IsAbstract && !typeof(Delegate).Adapt().IsAssignableFrom( parameter );
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

	public class DeferredExecutionMonitor
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
	}

	public class ComposeStrategy : BuilderStrategy
	{
		readonly DeferredExecutionMonitor monitor;

		public ComposeStrategy( [Required]DeferredExecutionMonitor monitor )
		{
			this.monitor = monitor;
		}

		[Required, Value( typeof(CompositionHostContext) )]
		public CompositionHost Host { [return: Required]get; set; }

		public override void PreBuildUp( IBuilderContext context )
		{
			var hasBuildPlan = context.HasBuildPlan();
			object existing;
			var reference = new KeyReference( this, context.BuildKey ).Item;
			if ( !hasBuildPlan && Host.TryGetExport( reference.Type, reference.Name, out existing ) )
			{
				context.Complete( existing );

				monitor.Execute( () =>
				{
					var checker = new Checked( reference, this );
					if ( checker.Item.Apply() )
					{
						var registry = context.New<IServiceRegistry>();
						registry.Register( new InstanceRegistrationParameter( reference.Type, existing, context.BuildKey.Name ) );
					}
				} );
			}
		}
	}

	public class ConventionStrategy : BuilderStrategy
	{
		[Persistent]
		class ConventionCandidateLocator : SpecificationAwareFactory<IBuilderContext, Type>
		{
			public ConventionCandidateLocator( [Required]BuildableTypeFromConventionLocator factory ) : this( InvalidBuildFromContextSpecification.Instance, factory ) { }

			ConventionCandidateLocator( [Required]InvalidBuildFromContextSpecification specification, [Required]BuildableTypeFromConventionLocator factory ) : base( specification, context => factory.Create( context.BuildKey.Type ) ) { }
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = new KeyReference( this, context.BuildKey ).Item;
			if ( new Checked( reference, this ).Item.Apply() )
			{
				var type = context.BuildKey.Type;
				context.New<ConventionCandidateLocator>().Create( context ).With( located =>
				{
					var mapped = new NamedTypeBuildKey( located, context.BuildKey.Name );

					var registry = context.New<IServiceRegistry>();

					registry.Register( new MappingRegistrationParameter( type, mapped.Type, context.BuildKey.Name ) );

					context.BuildKey = mapped;
				} );
			}
		}
	}
}