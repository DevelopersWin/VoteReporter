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

		public static T Locate<T>()
		{
			var locate = Location.Locate<T>();
			return locate.OrDefault( Activator.Activate<T> );
		}

		static IApplication Current => new CurrentApplication().Item;

		public static T Get<T>() => (T)Get( typeof(T) );
		
		public static object Get( [Required] Type type ) => Current.GetService( type );
	}
}