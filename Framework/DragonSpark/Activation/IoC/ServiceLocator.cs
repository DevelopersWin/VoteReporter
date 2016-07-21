using DragonSpark.Aspects;
using DragonSpark.Diagnostics.Logger;
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
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Activation.IoC
{
	[Disposable( ThrowObjectDisposedException = true )]
	public class ServiceLocator : ServiceLocatorImplBase
	{
		readonly ILogger logger;

		public ServiceLocator( IUnityContainer container ) : this( container, container.TryResolve<ILogger>() ?? Logging.Instance.Get( container ) ) {}

		public ServiceLocator( IUnityContainer container, ILogger logger )
		{
			Container = container;
			this.logger = logger;
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
				logger.Debug( Resources.ServiceLocator_NotRegistered, serviceType, key ?? Resources.Activator_None );
			}
			return result;
		}

		[Reference]
		public IUnityContainer Container { get; }
	}

	public sealed class UnityContainerFactory : AggregateFactoryBase<IUnityContainer>
	{
		readonly static ImmutableArray<ITransformer<IUnityContainer>> Default = new ITransformer<IUnityContainer>[] { DefaultUnityExtensions.Instance }.ToImmutableArray();

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();
		UnityContainerFactory() : base( () => new UnityContainer().Extend<DefaultBehaviorExtension>(), () => Default ) {}

		[Creator]
		public override IUnityContainer Create() => base.Create();
	}

	public class DefaultBehaviorExtension : UnityContainerExtension
	{
		protected override void Initialize()
		{
			var repository = new StrategyRepository( Context.Strategies );
			repository.Add( new StrategyEntry( new BuildKeyMonitorExtension(), UnityBuildStage.PreCreation, Priority.High ) );
			Container.RegisterInstance( Logging.Instance.Get( Container ) );
			Container.RegisterInstance<IBuildPlanRepository>( new BuildPlanRepository( SingletonBuildPlanPolicy.Instance.ToItem() ) );
			Container.RegisterInstance<IStrategyRepository>( repository );
		}
	}

	public abstract class UnityConfigurator : TransformerBase<IUnityContainer> {}

	public sealed class DefaultUnityExtensions : UnityConfigurator
	{
		public static DefaultUnityExtensions Instance { get; } = new DefaultUnityExtensions();
		DefaultUnityExtensions() {}

		public override IUnityContainer Create( IUnityContainer parameter ) => 
			parameter
				.Extend<InstanceTypeRegistrationMonitorExtension>()
				.Extend<DefaultRegistrationsExtension>()
				.Extend<ServicesIntegrationExtension>()
				.Extend<StrategyPipelineExtension>()	
				.Extend<CachingBuildPlanExtension>()
				.Extend<DefaultConstructorPolicyExtension>()
				;
	}

	/*public sealed class ServicesConfigurator : UnityConfigurator
	{
		public static ServicesConfigurator Instance { get; } = new ServicesConfigurator();
		ServicesConfigurator() {}

		public override IUnityContainer Create( IUnityContainer parameter ) => parameter.Extend<ServicesIntegrationExtension>();
	}*/

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

	class BuildPlanRepository : PurgingRepositoryBase<IBuildPlanPolicy>, IBuildPlanRepository
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

		protected override IEnumerable<StrategyEntry> Query() => Store.Purge().OrderBy( entry => entry.Stage ).ThenBy( entry => entry.Priority );

		protected override void OnAdd( StrategyEntry entry )
		{
			base.OnAdd( entry );
			strategies.Add( entry.Value, entry.Stage );
		}

		protected override StrategyEntry Create( IBuilderStrategy item ) => new StrategyEntry( item, UnityBuildStage.PreCreation );
	}
}