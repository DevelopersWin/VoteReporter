using DragonSpark.Extensions;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;

namespace DragonSpark.Activation.IoC
{
	public class ServicesIntegrationExtension : UnityContainerExtension
	{
		readonly IServiceProvider provider;
		readonly Func<ILogger> logger;
		readonly IBuildPlanRepository repository;
		readonly IStrategyRepository strategies;

		public ServicesIntegrationExtension( IServiceProvider provider, Func<ILogger> logger, IStrategyRepository strategies, IBuildPlanRepository repository )
		{
			this.provider = provider;
			this.logger = new FirstFactory<ILogger>( logger, provider.Get<ILogger> ).Create;
			this.repository = repository;
			this.strategies = strategies;
		}

		protected override void Initialize()
		{
			Container.RegisterInstance( logger );

			var entries = new[]
			{
				new StrategyEntry( new EnumerableResolutionStrategy( Container, provider ), UnityBuildStage.Creation, Priority.Higher ),
				new StrategyEntry( new ArrayResolutionStrategy( provider ), UnityBuildStage.Creation, Priority.AboveNormal )
			};
			entries.Each( strategies.Add );

			var policy = new ServicesBuildPlanPolicy( provider );
			Context.Strategies.Add( new ServicesStrategy( policy ), UnityBuildStage.PreCreation );
			
			repository.Add( policy );
		}
	}

	public class ServicesBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly IServiceProvider provider;

		public ServicesBuildPlanPolicy( IServiceProvider provider )
		{
			this.provider = provider;
		}

		public void BuildUp( IBuilderContext context )
		{
			var existing = provider.GetService( context.BuildKey.Type );
			context.Complete( existing );
		}
	}

	/*public class NativeTypesFactory : FactoryBase<Type[]>
	{
		public static NativeTypesFactory Instance { get; } = new NativeTypesFactory();

		protected override Type[] CreateItem() => new[] { typeof(AttributeProviderFactoryBase), typeof(MemberInfoProviderFactoryBase), typeof(MemberInfoAttributeProviderFactory), typeof(IAttributeProvider), typeof(IMemberInfoLocator) };
	}*/

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