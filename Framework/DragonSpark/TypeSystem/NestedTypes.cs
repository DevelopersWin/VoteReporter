using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;

namespace DragonSpark.TypeSystem
{
	public sealed class NestedTypes : CacheWithImplementedFactoryBase<TypeInfo, ImmutableArray<Type>>
	{
		readonly static Func<TypeInfo, bool> Specification = ApplicationTypeSpecification.Default.IsSatisfiedBy;

		public static NestedTypes Default { get; } = new NestedTypes();
		NestedTypes() {}

		protected override ImmutableArray<Type> Create( TypeInfo parameter ) =>
			parameter.Append( parameter.DeclaredNestedTypes ).Where( Specification ).AsTypes().ToImmutableArray();
	}
}