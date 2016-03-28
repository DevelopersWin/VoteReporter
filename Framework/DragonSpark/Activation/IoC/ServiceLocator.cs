using DragonSpark.Extensions;
using DragonSpark.Properties;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	[Disposable( ThrowObjectDisposedException = true )]
	public class ServiceLocator : ServiceLocatorImplBase
	{
		// public ServiceLocator( [Required]IUnityContainer container ) : this( container, container.Resolve<ILogger>() ) {}

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

		[Child]
		public IUnityContainer Container { get; }
		
		[Reference]
		public ILogger Logger { get; }
	}

	public abstract class UnityConfigurator : TransformerBase<IUnityContainer> {}

	/*public class DefaultUnityInstances : UnityConfigurator
	{
		readonly Func<Assembly[]> assemblySource;
		readonly Func<Type[]> types;
		readonly Func<BuildableTypeFromConventionLocator> locator;

		public DefaultUnityInstances( [Required] Func<Assembly[]> assemblySource, [Required] Func<Type[]> types, [Required] Func<BuildableTypeFromConventionLocator> locator )
		{
			this.assemblySource = assemblySource;
			this.types = types;
			this.locator = locator;
		}

		protected override IUnityContainer CreateItem( IUnityContainer parameter )
		{
			var instance = types();
			var assemblies = assemblySource();
			return parameter
				.RegisterInstance( assemblies )
				.RegisterInstance( instance )
				.RegisterInstance( locator() );
		}
	}*/

	public class DefaultUnityExtensions : UnityConfigurator
	{
		public static DefaultUnityExtensions Instance { get; } = new DefaultUnityExtensions();

		protected override IUnityContainer CreateItem( IUnityContainer parameter ) => 
			parameter
				.Extend<CachingBuildPlanExtension>()
				.Extend<DefaultRegistrationsExtension>()
				.Extend<StrategyPipelineExtension>()
				.Extend<ServicesIntegrationExtension>()
				.Extend<InstanceTypeRegistrationMonitorExtension>();
	}

	public class ServicesConfigurator : UnityConfigurator
	{
		readonly Func<IServiceProvider> provider;

		public ServicesConfigurator( [Required] Func<IServiceProvider> provider )
		{
			this.provider = provider;
		}

		protected override IUnityContainer CreateItem( IUnityContainer parameter ) => 
			parameter
				.RegisterInstance( provider() )
				.Extend<ServicesIntegrationExtension>();
	}

	/*public class IntegratedUnityContainerFactory : FactoryBase<IUnityContainer>
	{
		readonly Func<IServiceProvider> source;

		public IntegratedUnityContainerFactory( [Required]Assembly[] assemblies ) : this( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ) ) {}
		
		public IntegratedUnityContainerFactory( [Required]Type[] types ) : this( new Func<ContainerConfiguration>( new TypeBasedConfigurationContainerFactory( types ).Create ) ) {}

		// public IntegratedUnityContainerFactory( Func<Type[]> types ) : this( new CompositionHostFactory( types, Default<ITransformer<ContainerConfiguration>>.Items ) ) {}

		public IntegratedUnityContainerFactory( Func<ContainerConfiguration> configuration ) : this( new Func<IServiceProvider>( new Composition.ServiceLocatorFactory( configuration ).Create ) ) {}

		public IntegratedUnityContainerFactory( [Required] Func<IServiceProvider> source )
		{
			this.source = source;
		}

		protected override IUnityContainer CreateItem()
		{
			var provider = source();
			var factory = new UnityContainerFactory( provider.Get<Assembly[]>, provider.Get<Type[]>, provider.Get<BuildableTypeFromConventionLocator>, provider.Get<CompositionContext> );
			var result = factory.Create();
			return result;
		}
	}*/

	public class UnityContainerFactory : AggregateFactory<IUnityContainer>
	{
		public UnityContainerFactory( [Required] Func<IServiceProvider> provider )
			: base( () => new UnityContainer(),
				new ServicesConfigurator( provider ).Create,
				DefaultUnityExtensions.Instance.Create
			) {}
	}
}