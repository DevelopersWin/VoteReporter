using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class TypesFactory : DecoratedSourceCache<Assembly, ImmutableArray<Type>>
	{
		readonly static Func<Assembly, IEnumerable<Type>> All = AssemblyTypes.All.GetEnumerable;

		public static TypesFactory Default { get; } = new TypesFactory();
		TypesFactory() : base( array => All( array).ToImmutableArray() ) {}
	}
}