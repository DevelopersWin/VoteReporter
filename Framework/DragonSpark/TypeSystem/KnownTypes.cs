using DragonSpark.Application;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public sealed class KnownTypes<T> : SuppliedSource<Type, ImmutableArray<Type>>
	{
		public static KnownTypes<T> Default { get; } = new KnownTypes<T>();
		KnownTypes() : base( KnownTypes.Default.Get, typeof(T) ) {}
	}

	public sealed class KnownTypes : ParameterizedSingletonScope<Type, ImmutableArray<Type>>
	{
		public static KnownTypes Default { get; } = new KnownTypes();
		KnownTypes() : base( type => ApplicationTypes.Default.Get().Where( type.IsAssignableFrom ).ToImmutableArray() ) {}
	}
}