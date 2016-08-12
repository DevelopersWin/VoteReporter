using System;

namespace DragonSpark.Sources.Caching
{
	public class Cache<T> : Cache<object, T>, ICache<T>/*, IConfigurableCache<T>*/ where T : class
	{
		public Cache() {}
		public Cache( Func<object, T> create ) : base( create ) {}
	}
}