using System;
using System.Collections.Generic;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.TypeSystem
{
	public sealed class EnumerableTypes : Cache<Type, Type>
	{
		public static EnumerableTypes Default { get; } = new EnumerableTypes();
		EnumerableTypes() : base( new TypeLocator( i => i.ImplementsGeneric( typeof(IEnumerable<>) ) ).Get ) {}
	}
}