using DragonSpark.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System;

namespace DragonSpark.Activation
{
	public static class Services
	{
		static Services()
		{
			Initialize( CurrentServiceProvider.Instance );

			ServiceLocator.SetLocatorProvider( Get<IServiceLocator> );
		}

		public static void Initialize( [Required] IValue<IServiceProvider> provider ) => Provider = provider;

		static IValue<IServiceProvider> Provider { get; set; }

		/*static IServiceProvider Default { get; set; }

		static IServiceProvider Current => CurrentServiceProvider.Instance.Item ?? Default;*/
		
		public static T Get<T>() => Get<T>( typeof(T) );

		public static T Get<T>( [Required]Type type ) => (T)Get( type );

		public static object Get( [Required] Type type ) => Provider.Item.GetService( type );
	}

	public class ServiceProvider : CompositeServiceProvider
	{
		// public static ServiceProvider Instance { get; } = new ServiceProvider();

		public ServiceProvider() : this( new RecordingLoggerFactory() ) {}

		public ServiceProvider( RecordingLoggerFactory factory ) : base( new DefaultInstances( factory ), ActivatedServiceProvider.Instance ) {}
	}

	class DefaultInstances : InstanceServiceProvider
	{
		// public DefaultInstances() : this( new RecordingLoggerFactory() ) {}

		public DefaultInstances( RecordingLoggerFactory factory ) : this( factory.Create(), factory.History, factory.LevelSwitch, Activator.Instance, new DisposableRepository() ) {}

		public DefaultInstances( ILogger logger, ILoggerHistory history, LoggingLevelSwitch level, IActivator activator, IDisposableRepository repository ) : base( logger, history, level, activator, repository ) {}
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