using DragonSpark.Activation.IoC;
using Microsoft.Practices.Unity;
using System;

namespace DragonSpark.Extensions
{
	public static class UnityContainerExtensions
	{
		public static T Resolve<T>( this IUnityContainer container, Type type ) => (T)container.Resolve( type );

		// public static T Resolve<T>( this IUnityContainer @this, Func<T> @default ) => @this.IsRegistered<T>() ? @this.Resolve<T>() : @default();

		public static T TryResolve<T>(this IUnityContainer container) => (T)TryResolve( container, typeof(T) );

		public static object TryResolve(this IUnityContainer container, Type typeToResolve, string name = null ) => container.Resolve<ResolutionContext>().Execute( () => container.Resolve( typeToResolve, name ) );

		public static IUnityContainer Extend<TExtension>( this IUnityContainer @this ) where TExtension : UnityContainerExtension => @this.Extension<TExtension>().Container;

		public static TExtension Extension<TExtension>( this IUnityContainer container ) where TExtension : UnityContainerExtension => (TExtension)container.Extension( typeof(TExtension) );

		public static IUnityContainerExtensionConfigurator Extension( this IUnityContainer container, Type extensionType )
		{
			var configure = container.Configure( extensionType );
			return (IUnityContainerExtensionConfigurator)configure ?? Create( container, extensionType );
		}

		static UnityContainerExtension Create( IUnityContainer container, Type extensionType )
		{
			var extension = container.Resolve<UnityContainerExtension>( extensionType );
			var result = extension.WithSelf( container.AddExtension );
			return result;
		}
	}
}