using DragonSpark.Application;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public sealed class KnownTypesOf<T> : SuppliedSource<Type, ImmutableArray<Type>>
	{
		public static KnownTypesOf<T> Default { get; } = new KnownTypesOf<T>();
		KnownTypesOf() : base( KnownTypesOf.Default.Get, typeof(T) ) {}
	}

	public sealed class KnownTypesOf : ParameterizedSingletonScope<Type, ImmutableArray<Type>>
	{
		public static KnownTypesOf Default { get; } = new KnownTypesOf();
		KnownTypesOf() : this( Implementation.Instance.Get ) {}

		[UsedImplicitly]
		public KnownTypesOf( Func<Type, ImmutableArray<Type>> factory ) : base( factory ) {}

		public sealed class Implementation : ParameterizedItemSourceBase<Type, Type>
		{
			public static Implementation Instance { get; } = new Implementation();
			Implementation() : this( ApplicationTypes.Default ) {}

			readonly IEnumerable<Type> types;

			public Implementation( IEnumerable<Type> types )
			{
				this.types = types;
			}

			public override IEnumerable<Type> Yield( Type parameter ) => types.Where( parameter.IsAssignableFrom );
		}
	}
}