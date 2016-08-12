using DragonSpark.Extensions;
using DragonSpark.Setup;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using System;
using DragonSpark.Sources.Caching;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation.IoC
{
	public class ServicesIntegrationExtension : UnityContainerExtension, IDependencyLocatorKey
	{
		readonly Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;
		// readonly ISpecification<LocateTypeRequest> specification;
		readonly IStrategyRepository strategies;
		readonly IBuildPlanRepository buildPlans;

		public ServicesIntegrationExtension( IStrategyRepository strategies, IBuildPlanRepository buildPlans, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) /*: this( strategies, buildPlans, registry, new HasFactorySpecification().Inverse() ) {}

		ServicesIntegrationExtension( IStrategyRepository strategies, IBuildPlanRepository buildPlans, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, ISpecification<LocateTypeRequest> specification )*/
		{
			this.registry = registry;
			// this.specification = specification;
			this.strategies = strategies;
			this.buildPlans = buildPlans;
		}

		protected override void Initialize()
		{
			Container.RegisterInstance<IDependencyLocatorKey>( this );
			// Container.RegisterInstance( specification );

			var provider = new DependencyFactory( this, registry ).ToDelegate();
			var entries = new[]
			{
				new StrategyEntry( new EnumerableResolutionStrategy( Container, provider ), UnityBuildStage.Creation, Priority.Higher ),
				new StrategyEntry( new ArrayResolutionStrategy( provider ), UnityBuildStage.Creation, Priority.BeforeNormal )
			};
			entries.Each( strategies.Add );

			buildPlans.Add( new ServicesBuildPlanPolicy( provider ) );
		}
	}

	public class DependencyFactory : FactoryBase<Type, object>
	{
		readonly IDependencyLocatorKey locatorKey;
		readonly Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;
		readonly Condition condition = new Condition();

		public DependencyFactory( IDependencyLocatorKey locatorKey, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) : this( locatorKey, new Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>>( registry )/*, isActivated */) {}

		DependencyFactory( IDependencyLocatorKey locatorKey, Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry )
		{
			this.locatorKey = locatorKey;
			this.registry = registry;
		}

		public override object Create( Type parameter )
		{
			var serviceSource = DependencyLocators.Instance.Get().For( locatorKey );
			var result = serviceSource?.Invoke( parameter );
			if ( result != null && condition.Get( result ).Apply() )
			{
				registry.Value.Register( new InstanceRegistrationParameter( parameter, result ) );
			}
			return result;
		}
	}

	public class ServicesBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly Func<Type, object> provider;
		public ServicesBuildPlanPolicy( Func<Type, object> provider )
		{
			this.provider = provider;
		}

		public void BuildUp( IBuilderContext context ) => context.Complete( provider( context.BuildKey.Type ) );
	}

	/*public class ServicesStrategy : BuilderStrategy
	{
		readonly ServicesBuildPlanPolicy policy;
		
		public ServicesStrategy( [Required] ServicesBuildPlanPolicy policy )
		{
			this.policy = policy;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				policy.BuildUp( context );
			}
		}
	}*/
}