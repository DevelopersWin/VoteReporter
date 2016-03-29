using DragonSpark.Properties;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Composition.Hosting;

namespace DragonSpark.Activation.IoC
{
	/*public class FrozenDisposeContainerControlledLifetimeManager : ContainerControlledLifetimeManager
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		[Freeze]
		protected override void Dispose( bool disposing )
		{
			monitor.Apply();
			base.Dispose( disposing );
		}
	}*/

	public class ConfigureProviderCommand : Command<IServiceLocator>
	{
		readonly IUnityContainer container;
		readonly ILogger logger;

		public ConfigureProviderCommand( [Required]IUnityContainer container, [Required]ILogger logger )
		{
			this.container = container;
			this.logger = logger;
		}

		protected override void OnExecute( IServiceLocator parameter )
		{
			logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			// container.RegisterInstance( parameter, new FrozenDisposeContainerControlledLifetimeManager() );
		}
	}

	public class ServiceProviderFactory : ServiceProviderFactory<ConfigureProviderCommand, IServiceLocator>
	{
		public ServiceProviderFactory( [Required] Func<ContainerConfiguration> configuration ) 
			: base( new ServiceProviderSourceFactory( configuration ).Create ) {}
	}

	public class ServiceProviderSourceFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<IUnityContainer> source;

		public ServiceProviderSourceFactory( [Required] Func<ContainerConfiguration> configuration )
			: this( new Func<IServiceProvider>( new Composition.ServiceProviderSourceFactory( configuration ).Create ) ) {}

		public ServiceProviderSourceFactory( [Required] Func<IServiceProvider> provider ) : this( new Func<IUnityContainer>( new UnityContainerFactory( provider ).Create ) ) {}

		public ServiceProviderSourceFactory( [Required] Func<IUnityContainer> source )
		{
			this.source = source;
		}

		protected override IServiceProvider CreateItem()
		{
			var primary = new ServiceLocator( source() );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary );
			return result;
		}
	}
}