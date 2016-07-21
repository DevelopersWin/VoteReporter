using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using System;

namespace DragonSpark.Activation.IoC
{
	public class ServicesIntegrationExtension : UnityContainerExtension, IDependencyLocatorKey
	{
		readonly Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;
		readonly ISpecification<LocateTypeRequest> specification;
		readonly IStrategyRepository strategies;
		readonly IBuildPlanRepository buildPlans;

		public ServicesIntegrationExtension( IStrategyRepository strategies, IBuildPlanRepository buildPlans, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) : this( strategies, buildPlans, registry, new HasFactorySpecification().Inverse() ) {}

		ServicesIntegrationExtension( IStrategyRepository strategies, IBuildPlanRepository buildPlans, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, ISpecification<LocateTypeRequest> specification )
		{
			this.registry = registry;
			this.specification = specification;
			this.strategies = strategies;
			this.buildPlans = buildPlans;
		}

		protected override void Initialize()
		{
			Container.RegisterInstance<IDependencyLocatorKey>( this );
			Container.RegisterInstance( specification );

			var entries = new[]
			{
				// new StrategyEntry( new ServicesStrategy( policy ), UnityBuildStage.PreCreation, Priority.Higher ), 

				new StrategyEntry( new EnumerableResolutionStrategy( Container, this ), UnityBuildStage.Creation, Priority.Higher ),
				new StrategyEntry( new ArrayResolutionStrategy( this ), UnityBuildStage.Creation, Priority.BeforeNormal )
			};
			entries.Each( strategies.Add );

			buildPlans.Add( new ServicesBuildPlanPolicy( this, registry ) );
		}
	}

	public class ServicesBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly static Func<object, bool> DefaultActivated = ActivationProperties.IsActivatedInstanceSpecification.Default.ToDelegate();

		readonly IDependencyLocatorKey locatorKey;
		readonly Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;
		readonly Func<object, bool> isActivated;
		readonly Condition condition = new Condition();

		public ServicesBuildPlanPolicy( IDependencyLocatorKey locatorKey, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) : this( locatorKey, registry, DefaultActivated ) {}

		public ServicesBuildPlanPolicy( IDependencyLocatorKey locatorKey, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, Func<object, bool> isActivated ) : this( locatorKey, new Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>>( registry ), isActivated ) {}

		ServicesBuildPlanPolicy( IDependencyLocatorKey locatorKey, Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, Func<object, bool> isActivated )
		{
			this.locatorKey = locatorKey;
			this.registry = registry;
			this.isActivated = isActivated;
		}

		public void BuildUp( IBuilderContext context )
		{
			var existing = DependencyLocator.Instance.For( locatorKey )?.Invoke( context.BuildKey.Type );
			if ( existing != null )
			{
				if ( condition.Get( existing ).Apply() && isActivated( existing ) )
				{
					registry.Value.Register( new InstanceRegistrationParameter( context.BuildKey.Type, existing ) );
				}
			}
			
			context.Complete( existing );
		}
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