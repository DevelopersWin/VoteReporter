using System;
using DragonSpark.Activation;
using Microsoft.Practices.ServiceLocation;

namespace DragonSpark.Extensions
{
	public static class ServiceLocatorExtensions
	{
		public static void With<TService>( this IServiceLocation @this, Action<TService> action ) => @this.IsAvailable.IsTrue( () => @this.Locate<TService>().With( action ) );

		public static TResult With<TService, TResult>( this IServiceLocation @this, Func<TService, TResult> action ) => @this.IsAvailable ? @this.Locate<TService>().With( action ) : default( TResult );

		public static TService Locate<TService>( this IServiceLocation @this, string name = null ) => @this.IsAvailable ? @this.Item.GetInstance<TService>( name ) : default( TService );

		public static void Register<TFrom, TTo>( this IServiceLocation @this ) => @this.With<IServiceLocator>( x => x.Register<TFrom, TTo>() );

		public static void Register( this IServiceLocation @this, Type from, Type to ) => @this.With<IServiceLocator>( x => x.Register( @from, to ) );

		public static void Register( this IServiceLocation @this, Type type, object instance ) => @this.With<IServiceLocator>( x => x.Register( type, instance ) );

		public static void Register<TService>( this IServiceLocation @this, TService instance ) => @this.With<IServiceLocator>( x => x.Register( instance ) );

		public static void Register<TService>( this IServiceLocation @this, Func<TService> factory ) => @this.With<IServiceLocator>( x => x.RegisterFactory( typeof(TService), () => factory() ) );
	}
}