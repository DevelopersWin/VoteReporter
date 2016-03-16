using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

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

		[Child]
		public IUnityContainer Container { get; }
		
		[Reference]
		public ILogger Logger { get; }
	}

	public class ServiceLocatorFactory : FactoryBase<ServiceProviderParameter, IServiceLocator>
	{
		public static ServiceLocatorFactory Instance { get; } = new ServiceLocatorFactory( AssignLocationCommand.Instance );

		readonly ConfigureLocationCommand configure;
		readonly UnityContainerFactory factory;

		public ServiceLocatorFactory( ConfigureLocationCommand configure ) : this( configure, UnityContainerFactory.Instance ) {}

		public ServiceLocatorFactory( ConfigureLocationCommand configure, UnityContainerFactory factory )
		{
			this.configure = configure;
			this.factory = factory;
		}

		protected override IServiceLocator CreateItem( ServiceProviderParameter parameter )
		{
			var container = factory.Create( parameter );
			var result = new ServiceLocator( container );
			var commandParameter = new ConfigureLocationCommand.Parameter( result, result.Container, result.Logger );
			configure.ExecuteWith( commandParameter );
			return result;
		}
	}
}