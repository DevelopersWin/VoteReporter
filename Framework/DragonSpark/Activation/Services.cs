using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System;
using System.Linq;
using IServiceLocator = Microsoft.Practices.ServiceLocation.IServiceLocator;
using ServiceLocator = Microsoft.Practices.ServiceLocation.ServiceLocator;

namespace DragonSpark.Activation
{
	public static class Services
	{
		static Services()
		{
			Initialize( new ServiceProvider() );

			ServiceLocator.SetLocatorProvider( Get<IServiceLocator> );
		}

		public static void Initialize( [Required] IServiceProvider provider ) => Provider = provider;

		static IServiceProvider Provider { get; set; }

		public static IServiceProvider Current => CurrentServiceProvider.Instance.Item ?? Provider;
		
		public static T Get<T>() => Get<T>( typeof(T) );

		public static T Get<T>( [Required]Type type ) => (T)Get( type );

		public static object Get( [Required] Type type ) => new[] { Current, Provider }.Distinct().FirstWhere( provider => provider.GetService( type ) );
	}

	public class ServiceProvider : CompositeServiceProvider
	{
		public ServiceProvider() : base( new DefaultInstances(), ActivatedServiceProvider.Instance ) {}
	}

	class DefaultInstances : InstanceServiceProvider
	{
		public DefaultInstances() : this( new RecordingLoggerFactory() ) {}

		DefaultInstances( RecordingLoggerFactory factory ) : this( factory.Create(), factory.History, factory.LevelSwitch, Activator.Instance ) {}

		public DefaultInstances( ILogger logger, ILoggerHistory history, LoggingLevelSwitch level, IActivator activator ) : base( logger, history, level, activator ) {}
	}

	class ActivatedServiceProvider : IServiceProvider
	{
		public static ActivatedServiceProvider Instance { get; } = new ActivatedServiceProvider();

		readonly IActivator activator;

		ActivatedServiceProvider() : this( Activator.Instance ) {}

		ActivatedServiceProvider( [Required]IActivator activator )
		{
			this.activator = activator;
		}

		public object GetService( Type serviceType ) => activator.Activate<object>( serviceType );
	}
}