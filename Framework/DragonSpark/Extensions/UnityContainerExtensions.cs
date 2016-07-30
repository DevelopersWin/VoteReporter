using DragonSpark.Diagnostics.Logger;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using System;

namespace DragonSpark.Extensions
{
	public static class UnityContainerExtensions
	{
		public static T Resolve<T>( this IUnityContainer container, Type type ) => (T)container.Resolve( type, Items<ResolverOverride>.Default );

		public static T TryResolve<T>(this IUnityContainer container)
		{
			var tryResolve = TryResolve( container, typeof(T) );
			return (T)tryResolve;
		}

		public static object TryResolve( this IUnityContainer container, Type typeToResolve, string name = null )
		{
			try
			{
				return container.Resolve( typeToResolve, name, Items<ResolverOverride>.Default );
			}
			catch ( Exception exception )
			{
				Logging.Instance.Get( container ).Debug( exception, "Could not resolve {Type} and {Name}", typeToResolve, name );
				return null;
			}
		}

		public static IUnityContainer Extend<TExtension>( this IUnityContainer @this ) where TExtension : UnityContainerExtension => @this.Extension<TExtension>().Container;

		public static TExtension Extension<TExtension>( this IUnityContainer container ) where TExtension : UnityContainerExtension => (TExtension)container.Extension( typeof(TExtension) );

		public static IUnityContainerExtensionConfigurator Extension( this IUnityContainer container, Type extensionType )
		{
			var configure = container.Configure( extensionType );
			return (IUnityContainerExtensionConfigurator)configure ?? Create( container, extensionType );
		}

		static UnityContainerExtension Create( IUnityContainer container, Type extensionType )
		{
			var result = container.Resolve<UnityContainerExtension>( extensionType );
			container.AddExtension( result );
			return result;
		}
	}
}