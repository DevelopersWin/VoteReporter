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
		public static IParameterizedConfiguration<Type, ImmutableArray<Type>> Instance { get; } = new KnownTypes();
		KnownTypes() : base( type => ApplicationTypes.Instance.Value.Types.Where( type.Adapt().IsAssignableFrom ).ToImmutableArray() ) {}
	}
}