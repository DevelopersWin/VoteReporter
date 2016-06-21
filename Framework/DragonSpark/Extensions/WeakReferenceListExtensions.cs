using System;

namespace DragonSpark.Extensions
{
	public static class WeakReferenceListExtensions
	{
		public static T Get<T>( this WeakReference<T> @this ) where T : class
		{
			T result;
			return @this.TryGetTarget( out result ) ? result : null;
		}

		/*public static void CheckWith<TItem>( this IList<WeakReference<TItem>> target, TItem item, Action<TItem> action ) where TItem : class
		{
			if ( !target.Exists( item ) )
			{
				target.Add( new WeakReference<TItem>( item ) );
				action( item );
				// target.Loaded += new Loader<TFrameworkElement>( callback ).OnLoad;
			}
		}*/

		/*public static TItem[] Targets<TItem>( this IList<WeakReference<TItem>> target ) where TItem : class
		{
			TItem item;
			var result = target.AliveOnly().Select( x => x.TryGetTarget( out item ).With( y => item ) ).ToArray();
			return result;
		}

		public static IList<WeakReference<TItem>> AliveOnly<TItem>( this IList<WeakReference<TItem>> target ) where TItem : class
		{
			TItem item;
			var items = target.Where( x => x == null || !x.TryGetTarget( out item ) ).ToArray();
			items.Each( target.Remove );
			return target;
		}

		public static bool Exists<TItem>( this IList<WeakReference<TItem>> target, object item ) where TItem : class
		{
			var result = target.Targets().Contains( item );
			return result;
		}*/
	}
}