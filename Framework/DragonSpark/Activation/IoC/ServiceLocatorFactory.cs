using DragonSpark.Aspects;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Composition;
using System.Composition.Hosting;

namespace DragonSpark.Activation.IoC
{
	public class FrozenDisposeContainerControlledLifetimeManager : ContainerControlledLifetimeManager
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		[Freeze]
		protected override void Dispose( bool disposing )
		{
			monitor.Apply();
			base.Dispose( disposing );
		}
	}

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
			container.RegisterInstance( parameter, new FrozenDisposeContainerControlledLifetimeManager() );
		}
	}
	
	public class ServiceProvidersFactory : FactoryBase<IServiceProvider[]>
	{
		readonly Func<IUnityContainer> container;
		readonly Func<CompositionContext> source;

		public ServiceProvidersFactory( [Required] Func<ContainerConfiguration> configuration, [Required] Func<IUnityContainer> container )
			: this( new Func<CompositionContext>( new CompositionFactory( configuration ).Create ), container ) {}

		public ServiceProvidersFactory( [Required] Func<CompositionContext> source, [Required] Func<IUnityContainer> container )
		{
			this.container = container;
			this.source = source;
		}

		protected override IServiceProvider[] CreateItem()
		{
			var composition = new Composition.ServiceLocator( source() );
			var @default = new CurrentServiceProvider().Item;
			var combined = new CompositeServiceProvider( composition, @default );
			var primary = new ServiceLocator( container(), combined.Get<ILogger>() );
			var result = new[] { new InstanceServiceProvider( primary ), composition, primary, @default };
			return result;
		}
	}

	public class ServiceProviderFactory : ServiceProviderFactory<ConfigureProviderCommand, IServiceLocator>
	{
		public ServiceProviderFactory( [Required] Func<ContainerConfiguration> configuration, [Required] Func<IUnityContainer> source ) : base( new ServiceProvidersFactory( new Func<CompositionContext>( new CompositionFactory( configuration ).Create ), source ).Create ) {}
	}
}