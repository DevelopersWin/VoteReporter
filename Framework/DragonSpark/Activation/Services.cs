using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using Microsoft.Practices.ServiceLocation;
using System;

namespace DragonSpark.Activation
{
	public class GlobalServiceProvider : Configuration<IServiceProvider>
	{
		public static GlobalServiceProvider Instance { get; } = new GlobalServiceProvider();

		GlobalServiceProvider() : base( () => DefaultServiceProvider.Instance )
		{
			ServiceLocator.SetLocatorProvider( GetService<IServiceLocator> );
		}

		public T GetService<T>() => GetService<T>( typeof(T) );

		public T GetService<T>( Type type ) => Get().Get<T>( type );
	}

	public sealed class DefaultServiceProvider : IServiceProvider
	{
		public static DefaultServiceProvider Instance { get; } = new DefaultServiceProvider();
		DefaultServiceProvider() : this( Activator.Instance.Get ) {}

		readonly Func<IActivator> activator;

		DefaultServiceProvider( Func<IActivator> activator )
		{
			this.activator = activator;
		}

		public object GetService( Type serviceType ) => activator().Activate<object>( serviceType );
	}
}