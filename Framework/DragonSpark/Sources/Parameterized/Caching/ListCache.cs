using System;
using System.Collections.Generic;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class ListCache<T> : ListCache<object, T>, ICache<IList<T>>
	{
		// public static ListCache Default { get; } = new ListCache();

		public ListCache() {}
		public ListCache( Func<object, IList<T>> create ) : base( create ) {}
	}

	public class ListCache<TInstance, TItem> : Cache<TInstance, IList<TItem>> where TInstance : class
	{
		/*public static ListCache Default { get; } = new ListCache();*/

		public ListCache() : base( key => new List<TItem>() ) {}
		public ListCache( Func<TInstance, IList<TItem>> create ) : base( create ) {}
	}

	public class ListCache : ListCache<object>
	{
		public static ListCache Default { get; } = new ListCache();

		public ListCache() {}
		public ListCache( Func<object, IList<object>> create ) : base( create ) {}
	}
}