using System;
using System.Collections.Generic;

namespace DragonSpark.Sources.Caching
{
	public class ListCache : ListCache<object>
	{
		public static ListCache Default { get; } = new ListCache();

		public ListCache() {}
		public ListCache( Func<object, IList<object>> create ) : base( create ) {}
	}
}