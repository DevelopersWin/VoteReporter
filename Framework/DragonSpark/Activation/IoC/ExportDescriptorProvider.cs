using DragonSpark.Activation.IoC.Specifications;
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
using System.Diagnostics;

namespace DragonSpark.Activation.IoC
{
	public class ServiceProviderSpecificationFactory : FactoryBase<IServiceProvider, ISpecification<LocateTypeRequest>>
	{
		public static ServiceProviderSpecificationFactory Instance { get; } = new ServiceProviderSpecificationFactory();
		
		public override ISpecification<LocateTypeRequest> Create( IServiceProvider parameter ) => 
			parameter.Get<FactoryTypeLocator>().With( locator => new HasFactorySpecification( locator ).Inverse() ) ?? Runtime.Specifications.Specifications.Always;
	}

	public class ServicesIntegrationExtension : UnityContainerExtension
	{
		readonly IServiceProvider provider;
		readonly ILogger logger;
		readonly Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;
		readonly ISpecification<LocateTypeRequest> specification;
		readonly IStrategyRepository strategies;

		public ServicesIntegrationExtension( IServiceProvider provider, IStrategyRepository strategies, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) : this( provider, provider.Get<ILogger>(), strategies, registry, ServiceProviderSpecificationFactory.Instance.Create( provider ) ) {}

		ServicesIntegrationExtension( IServiceProvider provider, ILogger logger, IStrategyRepository strategies, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, ISpecification<LocateTypeRequest> specification )
		{
			this.provider = provider;
			this.logger = logger;
			this.registry = registry;
			this.specification = specification;
			this.strategies = strategies;
		}

		protected override void Initialize()
		{
			Container.RegisterInstance( logger );
			Container.RegisterInstance( specification );

			var policy = new ServicesBuildPlanPolicy( provider, registry );
			var entries = new[]
			{
				new StrategyEntry( new ServicesStrategy( policy ), UnityBuildStage.PreCreation, Priority.Higher ), 

				new StrategyEntry( new EnumerableResolutionStrategy( Container, provider ), UnityBuildStage.Creation, Priority.Higher ),
				new StrategyEntry( new ArrayResolutionStrategy( provider ), UnityBuildStage.Creation, Priority.BeforeNormal )
			};
			entries.Each( strategies.Add );

			
			// repository.Add( policy );
		}
	}

	public class ServicesBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly IServiceProvider provider;
		readonly Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry;
		readonly Func<object, bool> isActivated;

		public ServicesBuildPlanPolicy( IServiceProvider provider, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry ) : this( provider, registry, ActivationProperties.IsActivatedInstanceSpecification.Instance.IsSatisfiedBy ) {}

		public ServicesBuildPlanPolicy( IServiceProvider provider, Func<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, Func<object, bool> isActivated ) : this( provider, new Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>>( registry ), isActivated ) {}

		ServicesBuildPlanPolicy( IServiceProvider provider, Lazy<ServiceRegistry<ExternallyControlledLifetimeManager>> registry, Func<object, bool> isActivated )
		{
			this.provider = provider;
			this.registry = registry;
			this.isActivated = isActivated;
		}

		public void BuildUp( IBuilderContext context )
		{
			var existing = provider.GetService( context.BuildKey.Type );
			existing.With( o =>
			{
				Debug.WriteLine( $"Output = {o.GetType()}" );
				if ( new Checked( o, this ).Value.Apply() && isActivated( o ) )
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