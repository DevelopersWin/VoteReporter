using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Patterns.Threading;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	[Disposable( ThrowObjectDisposedException = true )]
	public class ServiceLocator : ServiceLocatorImplBase
	{
		public ServiceLocator( [Required]IUnityContainer container ) : this( container, container.Resolve<ILogger>() ) {}

		public ServiceLocator( [Required]IUnityContainer container, [Required]ILogger logger )
		{
			Container = container;
			Logger = logger;
		}

		public override IEnumerable<TService> GetAllInstances<TService>()
		{
			var enumerable = Container.IsRegistered<IEnumerable<TService>>() ? Container.Resolve<IEnumerable<TService>>() : Enumerable.Empty<TService>();
			var result = base.GetAllInstances<TService>().Union( enumerable ).ToArray();
			return result;
		}

		protected override IEnumerable<object> DoGetAllInstances( Type serviceType ) => Container.ResolveAll( serviceType ).ToArray();

		protected override object DoGetInstance( Type serviceType, string key )
		{
			var result = Container.TryResolve( serviceType, key );
			if ( result == null && !Container.IsRegistered( serviceType, key ) )
			{
				Logger.Debug( Resources.ServiceLocator_NotRegistered, serviceType, key ?? Resources.Activator_None );
			}
			return result;
		}

		[Reference]
		public IUnityContainer Container { get; }
		
		[Reference]
		public ILogger Logger { get; }
	}

	public abstract class UnityConfigurator : TransformerBase<IUnityContainer> {}

	public class DefaultUnityExtensions : UnityConfigurator
	{
		public static DefaultUnityExtensions Instance { get; } = new DefaultUnityExtensions();

		public override IUnityContainer Create( IUnityContainer parameter ) => 
			parameter
				.Extend<InstanceTypeRegistrationMonitorExtension>()
				.Extend<CachingBuildPlanExtension>()
				.Extend<DefaultRegistrationsExtension>()
				.Extend<StrategyPipelineExtension>()	
				.Extend<DefaultConstructorPolicyExtension>()
				;
	}

	public class ServicesConfigurator : UnityConfigurator
	{
		readonly Func<IServiceProvider> provider;

		public ServicesConfigurator( [Required] Func<IServiceProvider> provider )
		{
			this.provider = provider;
		}

		public override IUnityContainer Create( IUnityContainer parameter ) => 
			parameter
				.RegisterInstance( provider() )
				.Extend<ServicesIntegrationExtension>();
	}

	public class UnityContainerCoreFactory : FactoryBase<IUnityContainer>
	{
		public static UnityContainerCoreFactory Instance { get; } = new UnityContainerCoreFactory();

		[Creator]
		public override IUnityContainer Create() => new UnityContainer().Extend<DefaultBehaviorExtension>();
	}

	public class DefaultBehaviorExtension : UnityContainerExtension
	{
		protected override void Initialize()
		{
			var repository = new StrategyRepository( Context.Strategies );
			repository.Add( new StrategyEntry( new BuildKeyMonitorExtension(), UnityBuildStage.PreCreation, Priority.High ) );
			Container.RegisterInstance<IBuildPlanRepository>( new BuildPlanRepository( SingletonBuildPlanPolicy.Instance ) );
			Container.RegisterInstance<IStrategyRepository>( repository );
		}
	}

	public class StrategyEntry : Entry<IBuilderStrategy>
	{
		public StrategyEntry( [Required] IBuilderStrategy strategy, UnityBuildStage stage, Priority priority = Priority.Normal ) : base( strategy, priority )
		{
			Stage = stage;
		}

		public UnityBuildStage Stage { get; }
	}

	public interface IStrategyRepository : IEntryRepository<IBuilderStrategy> {}

	public interface IBuildPlanRepository : IRepository<IBuildPlanPolicy> {}

	class BuildPlanRepository : RepositoryBase<IBuildPlanPolicy>, IBuildPlanRepository
	{
		public BuildPlanRepository( params IBuildPlanPolicy[] items ) : base( new List<IBuildPlanPolicy>( items ) ) {}
	}

	class StrategyRepository : EntryRepositoryBase<StrategyEntry, IBuilderStrategy>, IStrategyRepository
	{
		readonly StagedStrategyChain<UnityBuildStage> strategies;

		public StrategyRepository( StagedStrategyChain<UnityBuildStage> strategies ) : this( strategies, new[]
			{
				new StrategyEntry( new BuildKeyMappingStrategy(), UnityBuildStage.TypeMapping ),
				new StrategyEntry( new HierarchicalLifetimeStrategy(), UnityBuildStage.Lifetime ),
				new StrategyEntry( new LifetimeStrategy(), UnityBuildStage.Lifetime ),
				new StrategyEntry( new BuildPlanStrategy(), UnityBuildStage.Creation ),
			} ) {}

		StrategyRepository( StagedStrategyChain<UnityBuildStage> strategies, IEnumerable<StrategyEntry> entry ) : base( entry.ToList() )
		{
			this.strategies = strategies;
		}

		protected override IEnumerable<StrategyEntry> Query() => Store.OrderBy( entry => entry.Stage ).ThenBy( entry => entry.Priority );

		protected override void OnAdd( StrategyEntry entry )
		{
			base.OnAdd( entry );
			strategies.Add( entry.Value, entry.Stage );
		}

		protected override StrategyEntry Create( IBuilderStrategy item ) => new StrategyEntry( item, UnityBuildStage.PreCreation );
	}
}