using System.Runtime.CompilerServices;

namespace DragonSpark.Activation.Sources.Caching
{
	class CacheRegistry<T> : ICacheRegistry<T>
	{
		public static CacheRegistry<T> Instance { get; } = new CacheRegistry<T>();

		readonly ConditionalWeakTable<object, ICache<T>> cache = new ConditionalWeakTable<object, ICache<T>>();

		public void Register( object key, ICache<T> instance ) => cache.Add( key, instance );

		public void Clear( object key, object instance )
		{
			ICache<T> property;
			if ( cache.TryGetValue( key, out property ) )
			{
				property.Remove( instance );
			}
		}
	}
}