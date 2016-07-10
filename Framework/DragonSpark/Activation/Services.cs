using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using System;
using DisposableRepository = DragonSpark.Runtime.DisposableRepository;

namespace DragonSpark.Activation
{
	public class GlobalServiceProvider : StoreServiceProvider
	{
		static GlobalServiceProvider()
		{
			ServiceLocator.SetLocatorProvider( Instance.Get<IServiceLocator> );
		}

		public static GlobalServiceProvider Instance { get; } = new GlobalServiceProvider();

		GlobalServiceProvider() : base( CurrentServiceProvider.Instance ) {}
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