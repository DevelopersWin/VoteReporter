using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public class KnownTypes : StructuredParameterizedConfiguration<Type, ImmutableArray<Type>>
	{
		public static KnownTypes Instance { get; } = new KnownTypes();
		KnownTypes() : base( type => DefaultTypeSystem.Instance.Get().Types.Where( type.Adapt().IsAssignableFrom ).ToImmutableArray() ) {}
	}
}