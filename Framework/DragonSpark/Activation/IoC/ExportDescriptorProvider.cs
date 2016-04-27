using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;

namespace DragonSpark.Activation.IoC
{
	public class ServiceProviderSpecificationFactory : FactoryBase<IServiceProvider, ISpecification<LocateTypeRequest>>
	{
		public static ServiceProviderSpecificationFactory Instance { get; } = new ServiceProviderSpecificationFactory();
		
		protected override ISpecification<LocateTypeRequest> CreateItem( IServiceProvider parameter ) => 
			parameter.Get<FactoryTypeRequestLocator>().With( locator => new HasFactorySpecification( locator ).Inverse() ) ?? AlwaysSpecification<LocateTypeRequest>.Instance;
	}

	public class ServicesIntegrationExtension : UnityContainerExtension
	{
		readonly IServiceProvider provider;
		readonly ILogger logger;
		readonly IBuildPlanRepository repository;
		readonly Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;
		readonly ISpecification<LocateTypeRequest> specification;
		readonly IStrategyRepository strategies;

		public ServicesIntegrationExtension( IServiceProvider provider, IStrategyRepository strategies, IBuildPlanRepository repository, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) : this( provider, provider.Get<ILogger>(), strategies, repository, registry, ServiceProviderSpecificationFactory.Instance.Create( provider ) ) {}

		ServicesIntegrationExtension( IServiceProvider provider, ILogger logger, IStrategyRepository strategies, IBuildPlanRepository repository, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, ISpecification<LocateTypeRequest> specification )
		{
			this.provider = provider;
			this.logger = logger;
			this.repository = repository;
			this.registry = registry;
			this.specification = specification;
			this.strategies = strategies;
		}

		protected override void Initialize()
		{
			Container.RegisterInstance( logger );
			Container.RegisterInstance( specification );

			var entries = new[]
			{
				new StrategyEntry( new EnumerableResolutionStrategy( Container, provider ), UnityBuildStage.Creation, Priority.Higher ),
				new StrategyEntry( new ArrayResolutionStrategy( provider ), UnityBuildStage.Creation, Priority.AboveNormal )
			};
			entries.Each( strategies.Add );

			var policy = new ServicesBuildPlanPolicy( provider, registry );
			Context.Strategies.Add( new ServicesStrategy( policy ), UnityBuildStage.PreCreation );
			
			repository.Add( policy );
		}
	}

	public class ServicesBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly IServiceProvider provider;
		readonly Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;

		public ServicesBuildPlanPolicy( IServiceProvider provider, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) : this( provider, new Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>>( registry ) ) {}

		public ServicesBuildPlanPolicy( IServiceProvider provider, Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry )
		{
			this.provider = provider;
			this.registry = registry;
		}

		public void BuildUp( IBuilderContext context )
		{
			var existing = provider.GetService( context.BuildKey.Type );
			existing.With( o =>
			{
				if ( new Checked( o, this ).Value.Apply() && ActivationProperties.IsActivatedInstanceSpecification.Instance.IsSatisfiedBy( o ) )
				{
					registry.Value.Register( new InstanceRegistrationParameter( context.BuildKey.Type, o ) );
				}
			} );
			context.Complete( existing );
		}
	}

	public class ServicesStrategy : BuilderStrategy
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
	}
}