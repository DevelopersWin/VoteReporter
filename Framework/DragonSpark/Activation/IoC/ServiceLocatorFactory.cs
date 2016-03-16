using DragonSpark.Aspects;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Activation.IoC
{
	public class FrozenDisposeContainerControlledLifetimeManager : ContainerControlledLifetimeManager
	{
		[Freeze]
		protected override void Dispose( bool disposing ) => base.Dispose( disposing );
	}

	public class ConfigureLocationCommand : Command<ConfigureLocationCommand.Parameter>
	{
		public static ConfigureLocationCommand Instance { get; } = new ConfigureLocationCommand();

		public class Parameter
		{
			public Parameter( [Required]IServiceLocator locator, [Required]IUnityContainer container, [Required]ILogger logger )
			{
				Locator = locator;
				Container = container;
				Logger = logger;
			}

			public IUnityContainer Container { get; }

			public IServiceLocator Locator { get; }

			public ILogger Logger { get; }
		}
		
		protected override void OnExecute( Parameter parameter )
		{
			parameter.Logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			parameter.Container.RegisterInstance( parameter.Locator, new FrozenDisposeContainerControlledLifetimeManager() );
		}
	}
}