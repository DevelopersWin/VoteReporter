using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using Serilog;
using System;

namespace DragonSpark.Extensions
{
	public class TryContextProperty : AttachedProperty<IUnityContainer, TryContext>
	{
		public static TryContextProperty Debug { get; } = new TryContextProperty( Container.Debug );

		public static TryContextProperty Verbose { get; } = new TryContextProperty( Container.Verbose );

		TryContextProperty( Container container ) : base( new Func<IUnityContainer, TryContext>( container.Create ) ) {}

		struct Container
		{
			public static Container Debug { get; } = new Container( logger => logger.Debug );

			public static Container Verbose { get; } = new Container( logger => logger.Verbose );

			readonly Func<ILogger, LogException> create;

			Container( Func<ILogger, LogException> create )
			{
				this.create = create;
			}

			public TryContext Create( IUnityContainer container )
			{
				var logger = container.Resolve<ILogger>( Items<ResolverOverride>.Default );
				var @delegate = create( logger );
				var result = new TryContext( @delegate );
				return result;
			}
		}
	}

	public static class UnityContainerExtensions
	{
		public static T Resolve<T>( this IUnityContainer container, Type type ) => (T)container.Resolve( type, Items<ResolverOverride>.Default );

		public static T TryResolve<T>(this IUnityContainer container) => (T)TryResolve( container, typeof(T) );

		public static object TryResolve(this IUnityContainer container, Type typeToResolve, string name = null ) => TryContextProperty.Debug.Get( container ).Invoke( new Context( container, typeToResolve, name ).Create ).Instance;

		struct Context
		{
			readonly IUnityContainer container;
			readonly Type typeToResolve;
			readonly string name;

			public Context( IUnityContainer container, Type typeToResolve, string name = null )
			{
				this.container = container;
				this.typeToResolve = typeToResolve;
				this.name = name;
			}

			public object Create() => container.Resolve( typeToResolve, name, Items<ResolverOverride>.Default );
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
			var extension = container.Resolve<UnityContainerExtension>( extensionType );
			var result = extension.WithSelf( container.AddExtension );
			return result;
		}
	}
}