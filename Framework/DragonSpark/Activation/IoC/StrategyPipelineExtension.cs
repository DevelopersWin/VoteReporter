using DragonSpark.Aspects;
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

	public class ConstructorExtension : UnityContainerExtension
	{
		readonly ResolvableTypeSpecification specification;

		public ConstructorExtension( ResolvableTypeSpecification specification )
		{
			this.specification = specification;
		}

		protected override void Initialize() => Context.Policies.SetDefault<IConstructorSelectorPolicy>( new ConstructorSelectorPolicy( specification ) );
	}

	public class CachingBuildPlanExtension : UnityContainerExtension
	{
		readonly ILogger logger;
		readonly IBuildPlanRepository repository;
		readonly ISpecification<LocateTypeRequest> specification;

		public CachingBuildPlanExtension( ILogger logger, IBuildPlanRepository repository, ISpecification<LocateTypeRequest> specification )
		{
			this.logger = logger;
			this.repository = repository;
			this.specification = specification;
		}

		protected override void Initialize()
		{
			var policies = repository.List();
			var creator = new Creator( Container ).Value.With( c => c.GetType() ) ?? ThreadAmbientContext.GetCurrent();
			var creators = new CachedCreatorPolicy( Context.Policies.Get<IBuildPlanCreatorPolicy>( null ), creator );
			var policy = new BuildPlanCreatorPolicy( new TryContext( logger ).Invoke, specification, policies, creators );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( policy );
		}

		class CachedCreatorPolicy : IBuildPlanCreatorPolicy
		{
			readonly IBuildPlanCreatorPolicy inner;
			readonly object creator;

			public CachedCreatorPolicy( [Required] IBuildPlanCreatorPolicy inner, object creator )
			{
				this.inner = inner;
				this.creator = creator;
			}

			class Plan : AssociatedStore<IBuildPlanPolicy>
			{
				public Plan( object creator, Type key, Func<IBuildPlanPolicy> create ) : base( creator, KeyFactory.Instance.CreateUsing( key, typeof(Plan) ).ToString(), create ) {}
			}

			public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey ) 
				=> new Plan( creator, context.BuildKey.Type, () => inner.CreatePlan( context, buildKey ) ).Value;
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

			public StrategyEntryFactory( [Required] MetadataLifetimeStrategy metadataLifetimeStrategy, [Required] ConventionStrategy conventionStrategy )
			{
				this.metadataLifetimeStrategy = metadataLifetimeStrategy;
				this.conventionStrategy = conventionStrategy;
			}

			protected override IEnumerable<StrategyEntry> CreateItem() => new[]
			{
				new StrategyEntry( metadataLifetimeStrategy, UnityBuildStage.Lifetime, Priority.Higher ),
				new StrategyEntry( conventionStrategy, UnityBuildStage.PreCreation )
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

			var strategyEntries = strategyRepository.List().Cast<StrategyEntry>();
			strategyEntries.Each( entry => Context.Strategies.Add( entry.Value, entry.Stage ) );
		}

		public class MetadataLifetimeStrategy : BuilderStrategy
		{
			readonly ILogger logger;
			readonly LifetimeManagerFactory factory;

			public MetadataLifetimeStrategy( [Required]ILogger logger, [Required]LifetimeManagerFactory factory )
			{
				this.logger = logger;
				this.factory = factory;
			}

			public override void PreBuildUp( IBuilderContext context )
			{
				var reference = new KeyReference( this, context.BuildKey ).Value;
				if ( new Checked( reference, this ).Value.Apply() )
				{
					var lifetimePolicy = context.Policies.GetNoDefault<ILifetimePolicy>( context.BuildKey, false );
					lifetimePolicy.Null( () =>
					{
						var lifetimeManager = factory.Create( reference.Type );
						lifetimeManager.With( manager =>
						{
							logger.Debug( $"'{GetType().Name}' is assigning a lifetime manager of '{manager.GetType()}' for type '{reference.Type}'." );

							context.PersistentPolicies.Set<ILifetimePolicy>( manager, reference );
						} );
					} );
				}
			}
		}
	}

	public class BuildableTypeFromConventionLocator : FactoryBase<Type, Type>
	{
		public static BuildableTypeFromConventionLocator Instance { get; } = new BuildableTypeFromConventionLocator();

		readonly Type[] types;
		readonly Func<Type, Type[]> strategy;
		readonly Func<Type, ITypeCandidateWeightProvider> weight;
		readonly ISpecification<Type> specification;

		public BuildableTypeFromConventionLocator( [Required]params Type[] types ) : this( types, AllTypesInCandidateAssemblyStrategy.Instance.Create, type => new TypeCandidateWeightProvider( type ), CanBuildSpecification.Instance.Or( ContainsSingletonSpecification.Instance ).Box<Type>(), CanBuildSpecification.Instance.Inverse<Type>() ) {}

		protected BuildableTypeFromConventionLocator( [Required]Type[] types, Func<Type, Type[]> strategy, Func<Type, ITypeCandidateWeightProvider> weight, [Required]ISpecification<Type> specification, [Required]ISpecification<Type> unbuildable ) : base( unbuildable )
		{
			this.types = types;
			this.strategy = strategy;
			this.weight = weight;
			this.specification = specification;
		}

		[Freeze]
		protected override Type CreateItem( Type parameter )
		{
			var adapter = parameter.Adapt();
			var name = parameter.Name.TrimStartOf( 'I' );
			var others = strategy( parameter );
			var order = weight( parameter );
			var result = 
				types
					.Union( others )
					.Where( adapter.IsAssignableFrom )
					.Where( specification.IsSatisfiedBy )
					.OrderByDescending( order.GetWeight )
					.FirstOrDefault( candidate => candidate.Name.Equals( name ) );
			return result;
		}
	}

	public interface ITypeCandidateWeightProvider
	{
		int GetWeight( Type candidate );
	}

	public class TypeCandidateWeightProvider : FactoryBase<Type, int>, ITypeCandidateWeightProvider
	{
		readonly Type subject;

		public TypeCandidateWeightProvider( Type subject )
		{
			this.subject = subject;
		}

		protected override int CreateItem( Type parameter ) => parameter.IsNested ? subject.GetTypeInfo().DeclaredNestedTypes.Contains( parameter.GetTypeInfo() ) ? 2 : -1 : 0;

		public int GetWeight( Type candidate ) => Create( candidate );
	}

	public abstract class TypeSelectionStrategyBase : FactoryBase<Type, Type[]>
	{
		protected TypeSelectionStrategyBase() {}

		protected TypeSelectionStrategyBase( ISpecification<Type> specification ) : base( specification ) {}
	}

	public class SelfStrategy : TypeSelectionStrategyBase
	{
		public static SelfStrategy Instance { get; } = new SelfStrategy();

		protected override Type[] CreateItem( Type parameter ) => parameter.ToItem();
	}

	public class SelfAndNestedStrategy : TypeSelectionStrategyBase
	{
		public static SelfAndNestedStrategy Instance { get; } = new SelfAndNestedStrategy();

		[Freeze]
		protected override Type[] CreateItem( Type parameter ) => parameter.Adapt().WithNested();
	}

	public class AllTypesInCandidateAssemblyStrategy : TypeSelectionStrategyBase
	{
		public static AllTypesInCandidateAssemblyStrategy Instance { get; } = new AllTypesInCandidateAssemblyStrategy( ApplicationAssemblySpecification.Instance.Box<Type>( type => type.Assembly() ) );

		public AllTypesInCandidateAssemblyStrategy( [Required] ISpecification<Type> specification ) : base( specification ) {}

		[Freeze]
		protected override Type[] CreateItem( Type parameter )
		{
			var types = TypesFactory.Instance.Create( parameter.Assembly().ToItem() );
			return types;
		}
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

	public class CanBuildSpecification : GuardedSpecificationBase<Type>
	{
		public static CanBuildSpecification Instance { get; } = new CanBuildSpecification();

		[Freeze]
		public override bool IsSatisfiedBy( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = parameter != typeof(object) && !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && !typeof(Delegate).Adapt().IsAssignableFrom( parameter ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class ValidConstructorSpecification : GuardedSpecificationBase<IBuilderContext>
	{
		public static ValidConstructorSpecification Instance { get; } = new ValidConstructorSpecification();

		public override bool IsSatisfiedBy( IBuilderContext parameter )
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
		readonly ConventionCandidateLocator locator;
		readonly IServiceRegistry registry;

		public class ConventionCandidateLocator : DecoratedFactory<IBuilderContext, Type>
		{
			static ISpecification<IBuilderContext> Specification { get; } = CanBuildSpecification.Instance.Box<IBuilderContext>( context => context.BuildKey.Type ).And( ValidConstructorSpecification.Instance ).Inverse<IBuilderContext>();

			public ConventionCandidateLocator( [Required]BuildableTypeFromConventionLocator factory ) : base( Specification, context => factory.Create( context.BuildKey.Type ) ) {}
		}

		public ConventionStrategy( [Required]ConventionCandidateLocator locator, [Required]IServiceRegistry registry )
		{
			this.locator = locator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = new KeyReference( this, context.BuildKey ).Value;
			if ( new Checked( reference, this ).Value.Apply() )
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