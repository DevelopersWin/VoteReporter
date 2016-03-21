using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;

namespace DragonSpark.Activation.IoC
{
	public class FrozenDisposeContainerControlledLifetimeManager : ContainerControlledLifetimeManager
	{
		[Freeze]
		protected override void Dispose( bool disposing ) => base.Dispose( disposing );
	}

	public class ConfigureProviderCommand : ConfigureProviderCommandBase<ConfigureProviderCommand.Context>
	{
		public static ConfigureProviderCommand Instance { get; } = new ConfigureProviderCommand();

		public class Context
		{
			public Context( [Required]IUnityContainer container, [Required]ILogger logger )
			{
				Container = container;
				Logger = logger;
			}

			public IUnityContainer Container { get; }

			public ILogger Logger { get; }
		}
		
		protected override void Configure( ProviderContext context )
		{
			context.Context.Logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			context.Provider.As<IServiceLocator>( locator => context.Context.Container.RegisterInstance( locator, new FrozenDisposeContainerControlledLifetimeManager() ) );
		}
	}


	public class ServiceLocatorFactory : FactoryBase<IServiceLocator>
	{
		readonly Func<IUnityContainer> container;

		public ServiceLocatorFactory( [Required] Func<IUnityContainer> container )
		{
			this.container = container;
		}

		protected override IServiceLocator CreateItem() => new ServiceLocator( container() );
	}

	public class ServiceProviderFactory : Setup.ServiceProviderFactory
	{
		public ServiceProviderFactory( [Required] Func<IUnityContainer> source ) : base( new ServiceLocatorFactory( source ).Create, ConfigureProviderCommand.Instance.Run ) {}
	}

	/*public class ServiceProviderFactory : ConfiguringFactory<IServiceProvider>
	{
		// public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory( ConfigureLocationCommand.Instance );

		public ServiceProviderFactory( Func<IUnityContainer> factory ) : this( new ServiceLocatorFactory( factory ).Create, ConfigureProviderCommand.Instance.Run ) {}

		public ServiceProviderFactory( Func<IServiceProvider> factory, Action<IServiceProvider> configure ) : base( factory, configure ) {}

		/*protected override IServiceLocator CreateItem( ServiceLocatorParameter parameter )
		{
			var container = factory.Create( parameter );
			var result = new ServiceLocator( container );
			var commandParameter = new ConfigureLocationCommand.Parameter( result, result.Container, result.Logger );
			configure.ExecuteWith( commandParameter );
			return result;
		}#1#

		/*protected override IServiceProvider CreateItem()
		{
			throw new NotImplementedException();
		}#1#
	}*/
}