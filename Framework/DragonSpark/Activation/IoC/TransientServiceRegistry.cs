using DragonSpark.Extensions;
using DragonSpark.Properties;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;

namespace DragonSpark.Activation.IoC
{
	public class TransientServiceRegistry : ServiceRegistry<TransientLifetimeManager>
	{
		public TransientServiceRegistry( IUnityContainer container, ILogger logger, LifetimeManagerFactory<TransientLifetimeManager> factory ) : base( container, logger, factory ) {}
	}

	public class PersistentServiceRegistry : ServiceRegistry<ContainerControlledLifetimeManager>
	{
		public PersistentServiceRegistry( IUnityContainer container, ILogger logger, LifetimeManagerFactory<ContainerControlledLifetimeManager> factory ) : base( container, logger, factory ) {}
	}

	public class ServiceRegistry<TLifetime> : ServiceRegistry where TLifetime : LifetimeManager
	{
		public ServiceRegistry( IUnityContainer container, ILogger logger, LifetimeManagerFactory<TLifetime> factory ) : base( container, logger, factory ) {}
	}

	public class ServiceRegistry : IServiceRegistry
	{
		readonly IUnityContainer container;
		readonly ILogger logger;
		readonly Func<Type, LifetimeManager> lifetimeFactory;

		public ServiceRegistry( IUnityContainer container, Type lifetimeFactoryType ) : this( container, container.Resolve<ILogger>(), t => container.Resolve<LifetimeManager>( lifetimeFactoryType ) ) { }

		public ServiceRegistry( IUnityContainer container, LifetimeManager lifetimeManager ) : this( container, container.Resolve<ILogger>(), type => lifetimeManager ) { }

		public ServiceRegistry( IUnityContainer container, ILogger logger, [Required]LifetimeManagerFactory factory ) : this( container, logger, factory.Create ) { }

		protected ServiceRegistry( [Required]IUnityContainer container, [Required]ILogger logger, [Required]Func<Type, LifetimeManager> lifetimeFactory )
		{
			this.container = container;
			this.logger = logger;
			this.lifetimeFactory = lifetimeFactory;
		}

		public bool IsRegistered( Type type ) => container.IsRegistered( type );

		public void Register( MappingRegistrationParameter parameter )
		{
			var lifetimeManager = lifetimeFactory( parameter.MappedTo ) ?? new TransientLifetimeManager();
			container.RegisterType( parameter.RequestedType, parameter.MappedTo, parameter.Name, lifetimeManager );
			logger.Debug( string.Format( Resources.ServiceRegistry_Registering, parameter.RequestedType, parameter.MappedTo, lifetimeManager.GetType().FullName ) );
		}

		public void Register( InstanceRegistrationParameter parameter )
		{
			var to = parameter.Instance.GetType();
			var mapping = string.Concat( parameter.RequestedType.FullName, to != parameter.RequestedType ? $" -> {to.FullName}" : string.Empty );
			var lifetimeManager = lifetimeFactory( to ) ?? new ContainerControlledLifetimeManager();
			logger.Debug( $"Registering Unity Instance: {mapping} ({lifetimeManager.GetType().FullName})" );
			container.RegisterInstance( parameter.RequestedType, parameter.Name, parameter.Instance, lifetimeManager );
		}

		public void RegisterFactory( FactoryRegistrationParameter parameter )
		{
			var lifetimeManager = lifetimeFactory( parameter.RequestedType ) ?? new TransientLifetimeManager();
			logger.Debug( $"Registering Unity Factory: {parameter.RequestedType} ({lifetimeManager.GetType().FullName})" );
			container.RegisterType( parameter.RequestedType, parameter.Name, lifetimeManager, new InjectionFactory( x => parameter.Factory() ) );
		}
	}
}