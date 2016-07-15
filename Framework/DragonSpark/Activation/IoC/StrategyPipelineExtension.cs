using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Delegate = System.Delegate;
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

	public class DefaultConstructorPolicyExtension : UnityContainerExtension
	{
		readonly ConstructorLocator store;

		public DefaultConstructorPolicyExtension( ConstructorLocator store )
		{
			this.store = store;
		}

		protected override void Initialize() => Context.Policies.SetDefault<IConstructorSelectorPolicy>( new ConstructorSelectorPolicy( store ) );
	}

	public class CachingBuildPlanExtension : UnityContainerExtension
	{
		readonly static IAtomicCache<object, IBuildPlanCreatorPolicy> Policies = new Cache<IBuildPlanCreatorPolicy>();

		readonly IBuildPlanRepository repository;
		readonly ISpecification<LocateTypeRequest> specification;

		public CachingBuildPlanExtension( IBuildPlanRepository repository, ISpecification<LocateTypeRequest> specification )
		{
			this.repository = repository;
			this.specification = specification;
		}

		protected override void Initialize()
		{
			var creator = Creator.Default.Get( Container )?.GetType() ?? Defaults.ExecutionContext();
			var policies = repository.List();
			var policy = new BuildPlanCreatorPolicy( Policies.GetOrSet( creator, Create ), policies, specification );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( policy );
		}

		IBuildPlanCreatorPolicy Create( object instance ) => new CachedCreatorPolicy( Context.Policies.Get<IBuildPlanCreatorPolicy>( null ) );

		class CachedCreatorPolicy : IBuildPlanCreatorPolicy
		{
			readonly IBuildPlanCreatorPolicy inner;

			readonly ICache<NamedTypeBuildKey, IBuildPlanPolicy> cache = new Cache<NamedTypeBuildKey, IBuildPlanPolicy>();

			public CachedCreatorPolicy( IBuildPlanCreatorPolicy inner )
			{
				this.inner = inner;
			}

			public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey )
			{
				var key = References.Keys.Create( buildKey );
				var result = cache.Contains( key ) ? cache.Get( key ) : cache.SetValue( key, inner.CreatePlan( context, buildKey ) );
				return result;
			}
		}
	}

	public static class References
	{
		public static EqualityReference<NamedTypeBuildKey> Keys { get; } = EqualityReference<NamedTypeBuildKey>.Instance;
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

			public override IEnumerable<StrategyEntry> Create() => new[]
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

			foreach ( var entry in strategyRepository.List().CastArray<StrategyEntry>() )
			{
				Context.Strategies.Add( entry.Value, entry.Stage );
			}
		}

		public class MetadataLifetimeStrategy : BuilderStrategy
		{
			readonly ILogger logger;
			readonly LifetimeManagerFactory factory;
			readonly Condition condition = new Condition();


			public MetadataLifetimeStrategy( [Required]ILogger logger, [Required]LifetimeManagerFactory factory )
			{
				this.logger = logger;
				this.factory = factory;
			}

			public override void PreBuildUp( IBuilderContext context )
			{
				var reference = References.Keys.Create( context.BuildKey );
				if ( condition.Get( reference ).Apply() )
				{
					var lifetimePolicy = context.Policies.GetNoDefault<ILifetimePolicy>( context.BuildKey, false );
					if ( lifetimePolicy == null )
					{
						var manager = factory.Create( reference.Type );
						if ( manager != null )
						{
							logger.Debug( "'{TypeName}' is assigning a lifetime manager of '{LifetimeManager}' for type '{Reference}'.", GetType().Name, manager.GetType(), reference.Type );

							context.PersistentPolicies.Set<ILifetimePolicy>( manager, reference );
						}
					}
				}
			}
		}
	}

	public class BuildableTypeFromConventionLocator : Cache<Type, Type>
	{
		public static BuildableTypeFromConventionLocator Instance { get; } = new BuildableTypeFromConventionLocator();

		BuildableTypeFromConventionLocator() : this( Items<Type>.Default ) {}

		public BuildableTypeFromConventionLocator( params Type[] types ) : base( new Factory( types ).Create ) {}

		[ApplyAutoValidation]
		protected class Factory : FactoryBase<Type, Type>
		{
			readonly Type[] types;
			readonly Func<Type, ITypeCandidateWeightProvider> weight;
			readonly ISpecification<Type> specification;

			public Factory( params Type[] types ) : this( types, type => new TypeCandidateWeightProvider( type ), CanInstantiateSpecification.Instance.Or( ContainsSingletonSpecification.Instance ), CanInstantiateSpecification.Instance.Inverse() ) {}

			protected Factory( Type[] types, Func<Type, ITypeCandidateWeightProvider> weight, ISpecification<Type> specification, ISpecification<Type> unbuildable ) : base( unbuildable )
			{
				this.types = types;
				this.weight = weight;
				this.specification = specification;
			}

			public override Type Create( Type parameter ) => Map( parameter ) ?? Search( parameter );

			static Type Map( Type parameter )
			{
				var name = $"{parameter.Namespace}.{ConventionCandidateNameFactory.Instance.Create( parameter )}";
				var result = name != parameter.FullName ? parameter.Assembly().GetType( name ) : null;
				return result;
			}

			Type Search( Type parameter )
			{
				var adapter = parameter.Adapt();
				var order = weight( parameter );
				var convention = new IsConventionCandidateSpecification( parameter );
				var result =
					types
						.Where( adapter.IsAssignableFrom )
						.Where( specification.IsSatisfiedBy )
						.OrderByDescending( order.GetWeight )
						.FirstOrDefault( convention.IsSatisfiedBy );
				return result;
			}
		}
	}

	class IsConventionCandidateSpecification : GuardedSpecificationBase<Type>
	{
		readonly string type;

		public IsConventionCandidateSpecification( Type type ) : this( type, ConventionCandidateNameFactory.Instance.ToDelegate() ) {}
		public IsConventionCandidateSpecification( Type type, Func<Type, string> sanitizer ) : this( sanitizer( type ) ) {}
		IsConventionCandidateSpecification( string type )
		{
			this.type = type;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter.Name.Equals( type );
	}

	class ConventionCandidateNameFactory : FactoryBase<Type, string>
	{
		public static ConventionCandidateNameFactory Instance { get; } = new ConventionCandidateNameFactory();

		public override string Create( Type parameter ) => parameter.Name.TrimStartOf( 'I' );
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

		public override int Create( Type parameter ) => parameter.IsNested ? subject.GetTypeInfo().DeclaredNestedTypes.Contains( parameter.GetTypeInfo() ) ? 2 : -1 : 0;

		public int GetWeight( Type candidate ) => Create( candidate );
	}

	/*public abstract class TypeSelectionStrategyBase : FactoryBase<Type, Type[]>
	{
		protected TypeSelectionStrategyBase() {}

		protected TypeSelectionStrategyBase( ISpecification<Type> specification ) : base( specification ) {}
	}*/

	public class SelfStrategy : StoreCache<Type, IEnumerable<Type>>
	{
		public static SelfStrategy Instance { get; } = new SelfStrategy();
		SelfStrategy() : base( EnumerableEx.Return ) {}
	}

	public sealed class SelfAndNestedStrategy : StoreCache<Type, IEnumerable<Type>>
	{
		public static SelfAndNestedStrategy Instance { get; } = new SelfAndNestedStrategy();
		SelfAndNestedStrategy() : base( type => type.Adapt().WithNested() ) {}
	}

	/*public class AllTypesInCandidateAssemblyStrategy : TypeSelectionStrategyBase
	{
		public static AllTypesInCandidateAssemblyStrategy Instance { get; } = new AllTypesInCandidateAssemblyStrategy();

		AllTypesInCandidateAssemblyStrategy() : base( ApplicationAssemblySpecification.Instance.Cast<Type>( type => type.Assembly() ) ) {}

		[Freeze]
		protected override Type[] CreateItem( Type parameter ) => TypesFactory.Instance.Create( parameter.Assembly().ToItem() );
	}*/

	[Persistent]
	public class ImplementedInterfaceFromConventionLocator : Cache<Type, Type>
	{
		public static ImplementedInterfaceFromConventionLocator Instance { get; } = new ImplementedInterfaceFromConventionLocator( typeof(IFactory), typeof(IFactoryWithParameter) );

		public ImplementedInterfaceFromConventionLocator( params Type[] ignore ) : base( new Factory( ignore ).Create ) {}

		protected class Factory : FactoryBase<Type, Type>
		{
			readonly ImmutableArray<Type> ignore;

			public Factory( params Type[] ignore )
			{
				this.ignore = ignore.ToImmutableArray();
			}

			public override Type Create( Type parameter )
			{
				var types = parameter.GetTypeInfo().ImplementedInterfaces.Except( ignore.ToArray() ).ToArray();
				foreach ( var type in types )
				{
					if ( parameter.Name.Contains( type.Name.TrimStartOf( 'I' ) ) )
					{
						return type;
					}
				}
				return null;
			}
		}
	}

	public class CanInstantiateSpecification : GuardedSpecificationBase<Type>
	{
		public static ISpecification<Type> Instance { get; } = new DelegatedSpecification<Type>( new CanInstantiateSpecification().Cached().Get );

		protected CanInstantiateSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = !info.IsGenericTypeDefinition && !info.ContainsGenericParameters && !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class InstantiableTypeSpecification : GuardedSpecificationBase<Type>
	{
		public static ISpecification<Type> Instance { get; } = new DelegatedSpecification<Type>( new InstantiableTypeSpecification().Cached().Get );
		InstantiableTypeSpecification() : this( new[] { typeof(Delegate), typeof(Array) }.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		readonly ImmutableArray<TypeAdapter> exempt;

		public InstantiableTypeSpecification( ImmutableArray<TypeAdapter> exempt )
		{
			this.exempt = exempt;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter != typeof(object) && !exempt.IsAssignableFrom( parameter );
	}

	/*public class ValidConstructorSpecification : GuardedSpecificationBase<IBuilderContext>
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
	}*/

	/*class KeyReference : Reference<NamedTypeBuildKey>
	{
		public KeyReference( object instance, NamedTypeBuildKey key ) : base( instance, key ) { }
	}*/

	public class ConventionStrategy : BuilderStrategy
	{
		readonly Condition condition = new Condition();

		readonly ConventionCandidateLocator locator;
		readonly IServiceRegistry registry;

		public class ConventionCandidateLocator : Cache<Type, Type>
		{
			static ISpecification<Type> Specification { get; } = InstantiableTypeSpecification.Instance.And( CanInstantiateSpecification.Instance.Inverse() );

			public ConventionCandidateLocator( BuildableTypeFromConventionLocator factory ) : base( new DelegatedFactory<Type, Type>( factory.ToDelegate(), Specification ).Create ) {}
		}

		public ConventionStrategy( ConventionCandidateLocator locator, IServiceRegistry registry )
		{
			this.locator = locator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = References.Keys.Create( context.BuildKey );
			if ( condition.Get( reference ).Apply() )
			{
				var from = context.BuildKey.Type;
				var convention = locator.Get( from );
				if ( convention != null )
				{
					context.BuildKey = new NamedTypeBuildKey( convention, context.BuildKey.Name );
					
					registry.Register( new MappingRegistrationParameter( from, context.BuildKey.Type, context.BuildKey.Name ) );
				}
			}
		}
	}
}