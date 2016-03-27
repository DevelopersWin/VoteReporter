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
			ServiceLocator.SetLocatorProvider( Get<IServiceLocator> );
		}

		class ServiceProvider : IServiceProvider
		{
			public static ServiceProvider Instance { get; } = new ServiceProvider();

			readonly IActivator activator;

			ServiceProvider() : this( Activator.Instance ) {}

			ServiceProvider( [Required]IActivator activator )
			{
				this.activator = activator;
			}

			public object GetService( Type serviceType ) => serviceType.Adapt().IsInstanceOfType( activator ) ? activator : activator.Activate<object>( serviceType );
		}

		static IServiceProvider Current => (IServiceProvider)new CurrentApplication().Item ?? ServiceProvider.Instance;

		public static T Get<T>() => Get<T>( typeof(T) );

		public static T Get<T>( [Required]Type type ) => (T)Get( type );
		
		public static object Get( [Required] Type type ) => Current.GetService( type );
	}
}