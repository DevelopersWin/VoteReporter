using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public static class Extensions
	{
		readonly static IDictionary<string, object> NoMetadata = new ReadOnlyDictionary<string, object>( new Dictionary<string, object>() );

		public static T TryGet<T>( this CompositionContext @this, string name = null ) => TryGet<T>( @this, typeof(T), name );

		public static T TryGet<T>( this CompositionContext @this, Type type, string name = null )
		{
			object existing;
			var result = @this.TryGetExport( type, name, out existing ) ? (T)existing : default(T);
			return result;
		}

		public static object Registered( this LifetimeContext @this, object instance )
		{
			instance.As<IDisposable>( @this.AddBoundInstance );
			return instance;
		}

		public static ExportDescriptor ToSharedDescriptor( this ISource<object> @this, IEnumerable<CompositionDependency> _ ) => @this.ToSingleton().ToDescriptor( _ );
		public static ExportDescriptor ToDescriptor( this Func<object> @this, IEnumerable<CompositionDependency> _ ) => ExportDescriptor.Create( @this.Activate, NoMetadata );
		static object Activate( this Func<object> @this, LifetimeContext context, CompositionOperation operation ) => @this();
	}
}