using System;

namespace DragonSpark.Runtime.Sources.Caching
{
	public class DecoratedCache<T> : DecoratedCache<object, T>
	{
		public DecoratedCache( Func<object, T> factory ) : base( factory ) {}
		public DecoratedCache( ICache<object, T> cache ) : base( cache ) {}
	}
}