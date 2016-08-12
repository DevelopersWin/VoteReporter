using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class ThreadLocalStoreCache<T> : ThreadLocalStoreCache<object, T>
	{
		public ThreadLocalStoreCache() {}
		public ThreadLocalStoreCache( Func<T> create ) : base( create ) {}

		public ThreadLocalStoreCache( Func<object, IAssignableSource<T>> create ) : base( create ) {}
		/*public ThreadLocalStoreCache() : this( () => default(T) ) {}
		public ThreadLocalStoreCache( Func<T> create ) : base( create ) {}

		protected ThreadLocalStoreCache( IAttachedPropertyStore<object, T> store ) : base( store ) {}*/

		// protected ThreadLocalAttachedProperty( Func<object, IWritableStore<T>> store ) : base( store ) {}
	}
}