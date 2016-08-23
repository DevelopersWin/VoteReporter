using System;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;

namespace DragonSpark.Extensions
{
	public class TypeAdapterCache : Cache<Type, TypeAdapter>
	{
		public static TypeAdapterCache Default { get; } = new TypeAdapterCache();

		TypeAdapterCache() : base( t => new TypeAdapter( t ) ) {}
	}
}