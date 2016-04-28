using DragonSpark.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
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

		public static void Initialize( [Required] IStore<IServiceProvider> provider ) => Provider = provider;

		static IStore<IServiceProvider> Provider { get; set; }

		public static T Get<T>() => Get<T>( typeof(T) );

		public static T Get<T>( [Required]Type type ) => (T)Get( type );

		public static object Get( [Required] Type type ) => Provider.Value.GetService( type );
	}

	public class ServiceProvider : CompositeServiceProvider
	{
		public ServiceProvider() : this( new RecordingLoggerFactory() ) {}

		public ServiceProvider( RecordingLoggerFactory factory ) : base( new DefaultInstances( factory ), ActivatedServiceProvider.Instance ) {}
	}

	class DefaultInstances : InstanceServiceProvider
	{
		public DefaultInstances( RecordingLoggerFactory factory ) : base( new IFactory[] { factory, FrameworkTypes.Instance }, factory.History, factory.LevelSwitch, Activator.Instance, new DisposableRepository() ) {}
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