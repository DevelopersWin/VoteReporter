using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using System;

namespace DragonSpark.Activation
{
	public sealed class GlobalServiceProvider : Configuration<IServiceProvider>
	{
		public static IConfiguration<IServiceProvider> Instance { get; } = new GlobalServiceProvider();

		GlobalServiceProvider() : base( () => DefaultServiceProvider.Instance )
		{
			ServiceLocator.SetLocatorProvider( GetService<IServiceLocator> );
		}

		public static T GetService<T>() => GetService<T>( typeof(T) );

		public static T GetService<T>( Type type ) => Instance.Get().Get<T>( type );
	}

	public sealed class DefaultServiceProvider : CompositeServiceProvider
	{
		public static IServiceProvider Instance { get; } = new DefaultServiceProvider();
		DefaultServiceProvider() : base( new SourceInstanceServiceProvider( GlobalServiceProvider.Instance, Activator.Instance, ApplicationParts.Instance, ApplicationAssemblies.Instance, ApplicationTypes.Instance, LoggingHistory.Instance.ToSource(), LoggingController.Instance.ToSource(), Logging.Instance.ToSource() ), new InstanceServiceProvider( SingletonLocator.Instance ), new DecoratedServiceProvider( Activator.Activate<object> ) ) {}
	}

	public delegate object ServiceSource( Type serviceType );

	public class DecoratedServiceProvider : IServiceProvider
	{
		readonly ServiceSource inner;

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( ServiceSource inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
	}
}