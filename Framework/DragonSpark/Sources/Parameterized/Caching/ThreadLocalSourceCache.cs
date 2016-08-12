using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class ThreadLocalSourceCache<T> : ThreadLocalSourceCache<object, T>
	{
		public ThreadLocalSourceCache() {}
		public ThreadLocalSourceCache( Func<T> create ) : base( create ) {}

		public ThreadLocalSourceCache( Func<object, IAssignableSource<T>> create ) : base( create ) {}
		/*public ThreadLocalStoreCache() : this( () => default(T) ) {}
		public ThreadLocalStoreCache( Func<T> create ) : base( create ) {}

		protected ThreadLocalStoreCache( IAttachedPropertyStore<object, T> store ) : base( store ) {}*/

		// protected ThreadLocalAttachedProperty( Func<object, IWritableStore<T>> store ) : base( store ) {}
	}
}