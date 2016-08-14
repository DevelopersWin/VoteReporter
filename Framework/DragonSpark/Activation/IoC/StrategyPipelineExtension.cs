using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Serilog;
using System;
using System.Collections.Generic;

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
		
		public CachingBuildPlanExtension( IBuildPlanRepository repository )
		{
			this.repository = repository;
		}

		protected override void Initialize()
		{
			var creator = (object)Origin.Default.Get( Container )?.GetType() ?? Execution.Current();
			var policies = repository.List();
			var policy = new BuildPlanCreatorPolicy( Policies.GetOrSet( creator, Create ), policies );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( policy );
		}

		IBuildPlanCreatorPolicy Create( object instance )
		{
			var buildPlanCreatorPolicy = Context.Policies.Get<IBuildPlanCreatorPolicy>( null );
			return new CachedCreatorPolicy( buildPlanCreatorPolicy );
		}

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
				var key = References.Keys.Get( buildKey );
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
		readonly DefaultStrategyEntries factory;

		public StrategyPipelineExtension( IStrategyRepository strategyRepository, DefaultStrategyEntries factory )
		{
			this.strategyRepository = strategyRepository;
			this.factory = factory;
		}

		protected override void Initialize()
		{
			factory.Get().Each( strategyRepository.Add );

			Context.Strategies.Clear();

			foreach ( var entry in strategyRepository.List().CastArray<StrategyEntry>() )
			{
				Context.Strategies.Add( entry.Get(), entry.Stage );
			}
		}

		public class DefaultStrategyEntries : SourceBase<IEnumerable<StrategyEntry>>
		{
			readonly MetadataLifetimeStrategy metadataLifetimeStrategy;
			readonly ConventionStrategy conventionStrategy;

			public DefaultStrategyEntries( MetadataLifetimeStrategy metadataLifetimeStrategy, ConventionStrategy conventionStrategy )
			{
				this.metadataLifetimeStrategy = metadataLifetimeStrategy;
				this.conventionStrategy = conventionStrategy;
			}

			public override IEnumerable<StrategyEntry> Get() => new[]
			{
				new StrategyEntry( metadataLifetimeStrategy, UnityBuildStage.Lifetime, Priority.Higher ),
				new StrategyEntry( conventionStrategy, UnityBuildStage.PreCreation )
			};
		}

		public class MetadataLifetimeStrategy : BuilderStrategy
		{
			readonly ILogger logger;
			readonly LifetimeManagerFactory factory;
			readonly Condition condition = new Condition();

			public MetadataLifetimeStrategy( ILogger logger, LifetimeManagerFactory factory )
			{
				this.logger = logger;
				this.factory = factory;
			}

			public override void PreBuildUp( IBuilderContext context )
			{
				var reference = References.Keys.Get( context.BuildKey );
				if ( condition.Get( reference ).Apply() )
				{
					var lifetimePolicy = context.Policies.GetNoDefault<ILifetimePolicy>( context.BuildKey, false );
					if ( lifetimePolicy == null )
					{
						var manager = factory.Get( reference.Type );
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

	public class ConventionStrategy : BuilderStrategy
	{
		readonly Condition condition = new Condition();

		readonly IServiceRegistry registry;

		public ConventionStrategy( IServiceRegistry registry )
		{
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = References.Keys.Get( context.BuildKey );
			if ( condition.Get( reference ).Apply() )
			{
				var from = context.BuildKey.Type;
				var convention = ConventionTypes.Instance.Get( from );
				if ( convention != null )
				{
					context.BuildKey = new NamedTypeBuildKey( convention, context.BuildKey.Name );
					
					registry.Register( new MappingRegistrationParameter( from, context.BuildKey.Type, context.BuildKey.Name ) );
				}
			}
		}
	}
}