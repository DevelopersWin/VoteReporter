using System;
using System.Collections.Generic;

namespace DragonSpark.Runtime.Sources.Caching
{
	public class SetCache<T> : SetCache<object, T>, ICache<ISet<T>>
	{
		public SetCache() {}
		public SetCache( Func<object, ISet<T>> create ) : base( create ) {}
	}
}