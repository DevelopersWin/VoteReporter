using DragonSpark.Aspects;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
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
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
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

	public class CachingBuildPlanExtension : UnityContainerExtension
	{
		readonly Func<ILogger> logger;

		public CachingBuildPlanExtension( [Required] Func<ILogger> logger )
		{
			this.logger = logger;
		}

		protected override void Initialize()
		{
			var policy = new CachedCreatorPolicy( Context.Policies.Get<IBuildPlanCreatorPolicy>( null ) );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( new BuildPlanCreatorPolicy( new TryContext( logger ).Try, Policies, policy ) );
			Context.Policies.SetDefault<IConstructorSelectorPolicy>( DefaultUnityConstructorSelectorPolicy.Instance );
		}

		public IList<IBuildPlanPolicy> Policies { get; } = new List<IBuildPlanPolicy> { new SingletonBuildPlanPolicy() };

		class CachedCreatorPolicy : IBuildPlanCreatorPolicy
		{
			readonly IBuildPlanCreatorPolicy inner;

			public CachedCreatorPolicy( [Required] IBuildPlanCreatorPolicy inner )
			{
				this.inner = inner;
			}

			class Plan : ThreadAmbientValue<IBuildPlanPolicy>
			{
				public Plan( Type key, Func<IBuildPlanPolicy> create ) : base( KeyFactory.Instance.CreateUsing( key, typeof(Plan) ).ToString(), create ) {}
			}

			public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey ) => new Plan( context.BuildKey.Type, () => inner.CreatePlan( context, buildKey ) ).Item;
		}
	}

	public class StrategyPipelineExtension : UnityContainerExtension
	{
		readonly IStrategyRepository strategyRepository;
		readonly StrategyEntryFactory factory;

		public class StrategyEntryFactory : FactoryBase<IEnumerable<StrategyEntry>>
		{
			readonly MetadataLifetimeStrategy metadataLifetimeStrategy;
			readonly ConventionStrategy conventionStrategy;
			readonly EnumerableResolutionStrategy enumerableResolutionStrategy;

			public StrategyEntryFactory( [Required] MetadataLifetimeStrategy metadataLifetimeStrategy, [Required] ConventionStrategy conventionStrategy, [Required]EnumerableResolutionStrategy enumerableResolutionStrategy )
			{
				this.metadataLifetimeStrategy = metadataLifetimeStrategy;
				this.conventionStrategy = conventionStrategy;
				this.enumerableResolutionStrategy = enumerableResolutionStrategy;
			}

			protected override IEnumerable<StrategyEntry> CreateItem() => new[]
			{
				new StrategyEntry( metadataLifetimeStrategy, UnityBuildStage.Lifetime, Priority.Higher ),
				new StrategyEntry( conventionStrategy, UnityBuildStage.PreCreation ),
				new StrategyEntry( enumerableResolutionStrategy, UnityBuildStage.Creation, Priority.Higher )
			};
		}

		
		public StrategyPipelineExtension( [Required]IStrategyRepository strategyRepository, [Required] StrategyEntryFactory factory )
		{
			this.strategyRepository = strategyRepository;
			this.factory = factory;
		}

		protected override void Initialize()
		{
			factory.Create().Each( strategyRepository.Add );

			Context.Strategies.Clear();

			strategyRepository.Get().Each( entry => Context.Strategies.Add( entry.Strategy, entry.Stage ) );

			//Context.Strategies.AddNew<BuildKeyMappingStrategy>( UnityBuildStage.TypeMapping );
			// Context.Strategies.Add( metadataLifetimeStrategy, UnityBuildStage.Lifetime ); // Insert
			// Context.Strategies.AddNew<HierarchicalLifetimeStrategy>( UnityBuildStage.Lifetime );
			// Context.Strategies.AddNew<LifetimeStrategy>( UnityBuildStage.Lifetime );
			// Context.Strategies.Add( defaultValueStrategy, UnityBuildStage.Lifetime );
			// Context.Strategies.Add( conventionStrategy, UnityBuildStage.PreCreation );
			//Context.Strategies.AddNew<ArrayResolutionStrategy>( UnityBuildStage.Creation );
			// Context.Strategies.Add( enumerableResolutionStrategy, UnityBuildStage.Creation ); // Insert
			// Context.Strategies.AddNew<BuildPlanStrategy>( UnityBuildStage.Creation );
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

		/*public class Builder<T> : FactoryBase<IBuilderContext, T>
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
		}*/
	}

	public class BuildableTypeFromConventionLocator : FactoryBase<Type, Type>
	{
		public static BuildableTypeFromConventionLocator Instance { get; } = new BuildableTypeFromConventionLocator();

		readonly Type[] types;
		readonly Func<Type, Type[]> strategy;
		readonly ISpecification<Type> specification;

		public BuildableTypeFromConventionLocator( [Required]params Type[] types ) : this( types, AllTypesInCandidateAssemblyStrategy.Instance.Create, CanBuildSpecification.Instance.Or( ContainsSingletonSpecification.Instance ).Wrap<Type>(), CanBuildSpecification.Instance.Inverse() ) {}

		protected BuildableTypeFromConventionLocator( [Required]Type[] types, Func<Type, Type[]> strategy, [Required]ISpecification<Type> specification, [Required]ISpecification<Type> unbuildable ) : base( unbuildable )
		{
			this.types = types;
			this.strategy = strategy;
			this.specification = specification;
		}

		[Freeze]
		protected override Type CreateItem( Type parameter )
		{
			var adapter = parameter.Adapt();
			var name = parameter.Name.TrimStartOf( 'I' );
			var others = strategy( parameter );
			var result = 
				types.Union( others )
				.Where( adapter.IsAssignableFrom )
				.Where( specification.IsSatisfiedBy )
				.FirstOrDefault( candidate => candidate.Name.StartsWith( name ) );
			return result;
		}
	}

	public abstract class TypeSelectionStrategyBase : FactoryBase<Type, Type[]>
	{
		protected TypeSelectionStrategyBase() {}

		protected TypeSelectionStrategyBase( ISpecification<Type> specification ) : base( specification ) {}
	}

	public class SelfAndNestedStrategy : TypeSelectionStrategyBase
	{
		public static SelfAndNestedStrategy Instance { get; } = new SelfAndNestedStrategy();

		protected override Type[] CreateItem( Type parameter ) => parameter.Adapt().WithNested();
	}

	public class AllTypesInCandidateAssemblyStrategy : TypeSelectionStrategyBase
	{
		public static AllTypesInCandidateAssemblyStrategy Instance { get; } = new AllTypesInCandidateAssemblyStrategy( ApplicationAssemblySpecification.Instance.Wrap<Type>( type => type.Assembly() ) );

		public AllTypesInCandidateAssemblyStrategy( [Required] ISpecification<Type> specification ) : base( specification ) {}

		protected override Type[] CreateItem( Type parameter ) => TypesFactory.Instance.Create( parameter.Assembly().ToItem() );
	}

	[Persistent]
	public class ImplementedInterfaceFromConventionLocator : FactoryBase<Type, Type>
	{
		readonly Type[] ignore;
		public static ImplementedInterfaceFromConventionLocator Instance { get; } = new ImplementedInterfaceFromConventionLocator( typeof(IFactory), typeof(IFactoryWithParameter) );

		public ImplementedInterfaceFromConventionLocator( [Required]params Type[] ignore )
		{
			this.ignore = ignore;
		}

		[Freeze]
		protected override Type CreateItem( Type parameter )
		{
			var result =
				parameter.GetTypeInfo().ImplementedInterfaces.Except( ignore ).ToArray().With( interfaces => 
					interfaces.FirstOrDefault( i => parameter.Name.Contains( i.Name.TrimStartOf( 'I' ) ) )
					/*??
					interfaces.FirstOrDefault( t => assemblies.Contains( t.Assembly() ) )*/
				);
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
			var result = parameter != typeof(object) && !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && !typeof(Delegate).Adapt().IsAssignableFrom( parameter ) && ( info.IsPublic || new AssemblyAttributeProvider( info.Assembly ).Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class ValidConstructorSpecification : SpecificationBase<IBuilderContext>
	{
		public static ValidConstructorSpecification Instance { get; } = new ValidConstructorSpecification();

		protected override bool Verify( IBuilderContext parameter )
		{
			IPolicyList containingPolicyList;
			var constructor = parameter.Policies.Get<IConstructorSelectorPolicy>( parameter.BuildKey, out containingPolicyList ).SelectConstructor( parameter, containingPolicyList );
			var result = constructor.With( IsValidConstructor );
			return result;
		}

		static bool IsValidConstructor( SelectedConstructor selectedConstructor ) => selectedConstructor.Constructor.GetParameters().All( pi => !pi.ParameterType.IsByRef );
	}

	class KeyReference : Reference<NamedTypeBuildKey>
	{
		public KeyReference( object instance, NamedTypeBuildKey key ) : base( instance, key ) { }
	}

	public class ConventionStrategy : BuilderStrategy
	{
		static ISpecification<IBuilderContext> Specification { get; } = new DecoratedSpecification<IBuilderContext>( CanBuildSpecification.Instance.Wrap<IBuilderContext>( context => context.BuildKey.Type ).And( ValidConstructorSpecification.Instance ) ).Inverse();

		readonly ConventionCandidateLocator locator;
		readonly IServiceRegistry registry;

		public class ConventionCandidateLocator : DecoratedFactory<IBuilderContext, Type>
		{
			public ConventionCandidateLocator( [Required]BuildableTypeFromConventionLocator factory ) : this( Specification, factory ) { }

			ConventionCandidateLocator( [Required]ISpecification<IBuilderContext> specification, [Required]BuildableTypeFromConventionLocator factory ) : base( specification, context => factory.Create( context.BuildKey.Type ) ) { }
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