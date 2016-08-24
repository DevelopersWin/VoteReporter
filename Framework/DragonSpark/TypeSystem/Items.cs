using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public static class Items<T>
	{
		static Items()
		{
			Default = (T[])Enumerable.Empty<T>();
			Immutable = Default.ToImmutableArray();
			List = Default.ToImmutableList();
		}

		public static T[] Default { get; }

		public static ImmutableArray<T> Immutable { get; }

		public static IList<T> List { get; }
	}
}