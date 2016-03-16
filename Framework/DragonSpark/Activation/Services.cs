using DragonSpark.Extensions;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using System;
using ServiceLocator = Microsoft.Practices.ServiceLocation.ServiceLocator;

namespace DragonSpark.Activation
{
	public static class Services
	{
		static Services()
		{
			Initialize( ServiceLocation.Instance );
		}

		public static void Initialize( IServiceLocation location )
		{
			Location = location;

			ServiceLocator.SetLocatorProvider( GetLocator );
		}

		public static IServiceLocator GetLocator() => Location.Item;

		public static IServiceLocation Location { get; private set; }

		/*public static T Locate<T>()
		{
			var locate = Location.Locate<T>();
			return locate.OrDefault( Activator.Activate<T> );
		}*/

		class ServiceProvider : IServiceProvider
		{
			public static ServiceProvider Instance { get; } = new ServiceProvider();

			readonly IActivator activator;

			ServiceProvider() : this( SystemActivator.Instance ) {}

			ServiceProvider( [Required]IActivator activator )
			{
				this.activator = activator;
			}

			public object GetService( Type serviceType ) => serviceType.Adapt().IsInstanceOfType( activator ) ? activator : activator.Activate( serviceType );
		}

		static IServiceProvider Current => (IServiceProvider)new CurrentApplication().Item ?? ServiceProvider.Instance;

		public static T Get<T>() => (T)Get( typeof(T) );
		
		public static object Get( [Required] Type type )
		{
			var service = Current.GetService( type );
			return service;
		}
	}
}