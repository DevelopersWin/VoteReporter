using System;

namespace DragonSpark.Activation.Sources.Caching
{
	public class SourceCache<T> : StoreCache<object, T>, ICache<T>
	{
		public SourceCache() : this( new WritableStoreCache<object, T>() ) {}
		public SourceCache( Func<object, T> create ) : this( new WritableStoreCache<object, T>( create ) ) {}

		public SourceCache( IStoreCache<object, T> inner ) : base( inner ) {}
	}
}