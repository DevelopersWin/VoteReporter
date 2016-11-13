using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Runtime
{
	public sealed class DefaultItemValueAlteration<T> : DelegatedAlteration<IEnumerable<T>>
	{
		public static DefaultItemValueAlteration<T> Default { get; } = new DefaultItemValueAlteration<T>();
		DefaultItemValueAlteration() : base( DefaultValueAlteration<IEnumerable<T>>.Default.Get ) {}
	}
}
