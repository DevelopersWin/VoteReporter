using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.TypeSystem
{
	public static class Items<T>
	{
		public static IEnumerable<T> Enumerable { get; } = System.Linq.Enumerable.Empty<T>();

		public static T[] Default { get; } = (T[])Enumerable;

		public static ImmutableArray<T> Immutable { get; } = Default.ToImmutableArray();
	}
}