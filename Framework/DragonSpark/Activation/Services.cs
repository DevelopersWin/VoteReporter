using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using System;

namespace DragonSpark.Activation
{
	public class GlobalServiceProvider : Configuration<IServiceProvider>
	{
		public static GlobalServiceProvider Instance { get; } = new GlobalServiceProvider();

		GlobalServiceProvider() : base( () => DefaultServiceProvider.Instance.Value )
		{
			ServiceLocator.SetLocatorProvider( GetService<IServiceLocator> );
		}

		public T GetService<T>() => GetService<T>( typeof(T) );

		public T GetService<T>( Type type ) => Get().Get<T>( type );
	}

	public sealed class DefaultServiceProvider : CompositeServiceProvider
	{
		public static IStore<IServiceProvider> Instance { get; } = new ExecutionContextStore<IServiceProvider>( () => new DefaultServiceProvider() );
		DefaultServiceProvider() : base( new InstanceContainerServiceProvider( ApplicationParts.Instance, ApplicationAssemblies.Instance, ApplicationTypes.Instance, LoggingHistory.Instance.ToStore(), LoggingController.Instance.ToStore(), Logging.Instance.ToStore() ), new DecoratedServiceProvider( Activator.Activate<object> ) ) {}
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