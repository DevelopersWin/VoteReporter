using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using System;

namespace DragonSpark.Activation
{
	public class GlobalServiceProvider : DelegatedStore<IServiceProvider>
	{
		readonly static GlobalServiceProvider Store = new GlobalServiceProvider();
		public static IServiceProvider Instance => Store.Value;

		GlobalServiceProvider() : base( () => ApplicationConfiguration.Instance.Value?.Services ?? DefaultServiceProvider.Instance )
		{
			ServiceLocator.SetLocatorProvider( GetService<IServiceLocator> );
		}

		public T GetService<T>() => GetService<T>( typeof(T) );

		public T GetService<T>( Type type ) => Get().Get<T>( type );
	}

	public sealed class DefaultServiceProvider : CompositeServiceProvider
	{
		readonly static IStore<IServiceProvider> Store = new ExecutionContextStore<IServiceProvider>( () => new DefaultServiceProvider() );
		public static IServiceProvider Instance => Store.Value;
		DefaultServiceProvider() : base( new InstanceContainerServiceProvider( ApplicationConfiguration.Instance, ApplicationConfiguration.Instance.Services, ApplicationParts.Instance, ApplicationAssemblies.Instance, ApplicationTypes.Instance, LoggingHistory.Instance.ToStore(), LoggingController.Instance.ToStore(), Logging.Instance.ToStore() ), new DecoratedServiceProvider( Activator.Activate<object> ) ) {}
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