using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
		readonly ConstructorLocator locator;

		public DefaultConstructorPolicyExtension( ConstructorLocator locator )
		{
			this.locator = locator;
		}

		protected override void Initialize() => Context.Policies.SetDefault<IConstructorSelectorPolicy>( new ConstructorSelectorPolicy( locator ) );
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
			var creator = Container.Get( Creator.Property )?.GetType() ?? Execution.GetCurrent();
			var creators = new CachedCreatorPolicy( Context.Policies.Get<IBuildPlanCreatorPolicy>( null ), creator );
			var policy = new BuildPlanCreatorPolicy( new TryContext( logger ).Invoke, specification, policies, creators );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( policy );
		}

		class CachedCreatorPolicy : IBuildPlanCreatorPolicy
		{
			readonly static AttachedProperty<ConditionalWeakTable<Type, IBuildPlanPolicy>> Property = new AttachedProperty<ConditionalWeakTable<Type, IBuildPlanPolicy>>( ActivatedAttachedPropertyStore<object, ConditionalWeakTable<Type, IBuildPlanPolicy>>.Instance );
			// readonly static ConditionalWeakTable<Type, AttachedProperty<IBuildPlanPolicy>> Properties = new ConditionalWeakTable<Type, AttachedProperty<IBuildPlanPolicy>>();
			readonly IBuildPlanCreatorPolicy inner;
			readonly object creator;

			public CachedCreatorPolicy( [Required] IBuildPlanCreatorPolicy inner, object creator )
			{
				this.inner = inner;
				this.creator = creator;
			}

			/*class Plan : AssociatedStore<IBuildPlanPolicy>
			{
				public Plan( object creator, Type key, Func<IBuildPlanPolicy> create ) : base( creator, KeyFactory.Instance.ToString( key, typeof(Plan) ), create ) {}
			}

			public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey )
			{
				return new Plan( creator, context.BuildKey.Type, () => inner.CreatePlan( context, buildKey ) ).Value;
			}*/

			public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey ) => 
				Property.Get( creator ).GetValue( context.BuildKey.Type, o => inner.CreatePlan( context, buildKey ) );

			/*class Plan : AttachedProperty<ConditionalWeakTable<Type, IBuildPlanPolicy>>
			{}*/
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

			var strategyEntries = strategyRepository.List().Cast<StrategyEntry>();
			strategyEntries.Each( entry => Context.Strategies.Add( entry.Value, entry.Stage ) );
		}

		public class MetadataLifetimeStrategy : BuilderStrategy
		{
			readonly EqualityReference<NamedTypeBuildKey> property = new EqualityReference<NamedTypeBuildKey>();

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
				var reference = property.From( context.BuildKey );
				if ( reference.Get( condition ).Apply() )
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

	/*public class AdditionalTypesStrategyConfiguration : ConfigurationBase<TypeSelectionStrategyBase>
	{
		public AdditionalTypesStrategyConfiguration() : base( NoTypesStrategy.Instance ) {}
	}*/

	public class BuildableTypeFromConventionLocator : FactoryBase<Type, Type>
	{
		public static BuildableTypeFromConventionLocator Instance { get; } = new BuildableTypeFromConventionLocator();

		readonly Type[] types;
		readonly Func<Type, ITypeCandidateWeightProvider> weight;
		readonly ISpecification<Type> specification;

		public BuildableTypeFromConventionLocator( [Required]params Type[] types ) : this( types, type => new TypeCandidateWeightProvider( type ), CanInstantiateSpecification.Instance.Or( ContainsSingletonSpecification.Instance ), CanInstantiateSpecification.Instance.Inverse() ) {}

		protected BuildableTypeFromConventionLocator( [Required]Type[] types, Func<Type, ITypeCandidateWeightProvider> weight, [Required]ISpecification<Type> specification, [Required]ISpecification<Type> unbuildable ) : base( unbuildable )
		{
			this.types = types;
			this.weight = weight;
			this.specification = specification;
		}

		static Type Map( Type parameter )
		{
			var name = $"{parameter.Namespace}.{ConventionCandidateNameFactory.Instance.Create( parameter )}";
			var result = name != parameter.FullName ? parameter.Assembly().GetType( name ) : null;
			return result;
		}

		[Freeze]
		public override Type Create( Type parameter ) => Map( parameter ) ?? Search( parameter );

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

	class IsConventionCandidateSpecification : GuardedSpecificationBase<Type>
	{
		readonly string type;

		public IsConventionCandidateSpecification( Type type ) : this( type, ConventionCandidateNameFactory.Instance.Create ) {}
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

	public abstract class TypeSelectionStrategyBase : FactoryBase<Type, Type[]>
	{
		protected TypeSelectionStrategyBase() {}

		protected TypeSelectionStrategyBase( ISpecification<Type> specification ) : base( specification ) {}
	}

	public class SelfStrategy : TypeSelectionStrategyBase
	{
		public static SelfStrategy Instance { get; } = new SelfStrategy();

		SelfStrategy() {}

		public override Type[] Create( Type parameter ) => parameter.ToItem();
	}

	public class SelfAndNestedStrategy : TypeSelectionStrategyBase
	{
		public static SelfAndNestedStrategy Instance { get; } = new SelfAndNestedStrategy();

		SelfAndNestedStrategy() {}

		[Freeze]
		public override Type[] Create( Type parameter ) => parameter.Adapt().WithNested();
	}

	/*public class AllTypesInCandidateAssemblyStrategy : TypeSelectionStrategyBase
	{
		public static AllTypesInCandidateAssemblyStrategy Instance { get; } = new AllTypesInCandidateAssemblyStrategy();

		AllTypesInCandidateAssemblyStrategy() : base( ApplicationAssemblySpecification.Instance.Cast<Type>( type => type.Assembly() ) ) {}

		[Freeze]
		protected override Type[] CreateItem( Type parameter ) => TypesFactory.Instance.Create( parameter.Assembly().ToItem() );
	}*/

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
		public override Type Create( Type parameter )
		{
			var result =
				parameter.GetTypeInfo().ImplementedInterfaces.Except( ignore ).ToArray().With( interfaces => 
					interfaces.FirstOrDefault( i => parameter.Name.Contains( i.Name.TrimStartOf( 'I' ) ) )
				);
			return result;
		}
	}

	public class CanInstantiateSpecification : GuardedSpecificationBase<Type>
	{
		public static CanInstantiateSpecification Instance { get; } = new CanInstantiateSpecification();

		// [Freeze]
		public override bool IsSatisfiedBy( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class InstantiableTypeSpecification : GuardedSpecificationBase<Type>
	{
		public static InstantiableTypeSpecification Instance { get; } = new InstantiableTypeSpecification();

		readonly TypeAdapter[] exempt;

		public InstantiableTypeSpecification() : this( new[] { typeof(Delegate), typeof(Array) }.Select( type => type.Adapt() ).ToArray() ) {}

		public InstantiableTypeSpecification( TypeAdapter[] exempt )
		{
			this.exempt = exempt;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter != typeof(object) && exempt.All( adapter => !adapter.IsAssignableFrom( parameter ) );
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
		readonly EqualityReference<NamedTypeBuildKey> property = new EqualityReference<NamedTypeBuildKey>();
		readonly Condition condition = new Condition();

		readonly ConventionCandidateLocator locator;
		readonly IServiceRegistry registry;

		public class ConventionCandidateLocator : DelegatedFactory<IBuilderContext, Type>
		{
			static ISpecification<IBuilderContext> Specification { get; } = InstantiableTypeSpecification.Instance.And(
				CanInstantiateSpecification.Instance.Inverse() ).Cast<IBuilderContext>( context => context.BuildKey.Type )/*.And( ValidConstructorSpecification.Instance.Inverse() )*/;

			public ConventionCandidateLocator( [Required]BuildableTypeFromConventionLocator factory ) : base( Specification, context => factory.Create( context.BuildKey.Type ) ) {}
		}

		public ConventionStrategy( [Required]ConventionCandidateLocator locator, [Required]IServiceRegistry registry )
		{
			this.locator = locator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = property.From( context.BuildKey );
			if ( reference.Get( condition ).Apply() )
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