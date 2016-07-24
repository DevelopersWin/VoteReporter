using DragonSpark.Aspects;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
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
		[Reference]
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
		readonly static IConfigurations<IUnityContainer> Default = new Configurations<IUnityContainer>( DefaultUnityExtensions.Instance );

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();
		UnityContainerFactory() : base( () => new UnityContainer().Extend<DefaultBehaviorExtension>(), Default ) {}

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

		public override IUnityContainer Get( IUnityContainer parameter ) => 
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

	public interface IStrategyRepository : IRepository<IBuilderStrategy>, IComposable<StrategyEntry> {}

	public interface IBuildPlanRepository : IRepository<IBuildPlanPolicy> {}

	class BuildPlanRepository : RepositoryBase<IBuildPlanPolicy>, IBuildPlanRepository
	{
		public BuildPlanRepository( params IBuildPlanPolicy[] items ) : base( new PurgingCollection<IBuildPlanPolicy>( items ) ) {}
	}

	public class StoreCollection<TStore, TInstance> : CollectionBase<TStore> where TStore : IStore<TInstance>
	{
		public StoreCollection() {}
		public StoreCollection( IEnumerable<TStore> items ) : base( items ) {}
		public StoreCollection( ICollection<TStore> source ) : base( source ) {}

		public ImmutableArray<TInstance> Instances() => Query.Select( entry => entry.Value ).ToImmutableArray();
	}

	public class StrategyRepository : RepositoryBase<StrategyEntry>, IStrategyRepository
	{
		readonly static StrategyEntry[] DefaultEntries = {
															 new StrategyEntry( new BuildKeyMappingStrategy(), UnityBuildStage.TypeMapping ),
															 new StrategyEntry( new HierarchicalLifetimeStrategy(), UnityBuildStage.Lifetime ),
															 new StrategyEntry( new LifetimeStrategy(), UnityBuildStage.Lifetime ),
															 new StrategyEntry( new BuildPlanStrategy(), UnityBuildStage.Creation ),
														 };

		readonly StagedStrategyChain<UnityBuildStage> strategies;

		readonly StrategyEntryCollection collection;

		public StrategyRepository( StagedStrategyChain<UnityBuildStage> strategies ) : this( strategies, DefaultEntries ) {}

		StrategyRepository( StagedStrategyChain<UnityBuildStage> strategies, IEnumerable<StrategyEntry> entries ) : this( strategies, new StrategyEntryCollection( entries ) ) {}

		StrategyRepository( StagedStrategyChain<UnityBuildStage> strategies, StrategyEntryCollection collection ) : base( collection )
		{
			this.strategies = strategies;
			this.collection = collection;
		}

		protected override void OnAdd( StrategyEntry entry )
		{
			base.OnAdd( entry );
			strategies.Add( entry.Value, entry.Stage );
		}

		public void Add( IBuilderStrategy instance ) => Add( new StrategyEntry( instance, UnityBuildStage.PreCreation ) );

		ImmutableArray<IBuilderStrategy> IRepository<IBuilderStrategy>.List() => collection.Instances();
	}
}