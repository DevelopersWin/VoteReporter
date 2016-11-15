using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class Implementations : ParameterizedSourceCache<Type, Type, ImmutableArray<Type>>
	{
		public static Implementations Default { get; } = new Implementations();
		Implementations() : base( type => new Implementation( type ).ToCache() ) {}

		public sealed class Implementation : ParameterizedItemSourceBase<Type, Type>
		{
			readonly ImmutableArray<Type> candidates;
			
			public Implementation( Type referencedType ) : this( referencedType.Append( AllInterfaces.Default.GetFixed( referencedType ) ).ToImmutableArray() ) {}

			[UsedImplicitly]
			public Implementation( ImmutableArray<Type> candidates )
			{
				this.candidates = candidates;
			}

			public override IEnumerable<Type> Yield( Type parameter )
			{
				var result = candidates
					.Introduce( parameter,
								tuple =>
								{
									var first = tuple.Item1.GetTypeInfo();
									var second = tuple.Item2.GetTypeInfo();
									var match = first.IsGenericType && second.IsGenericType && tuple.Item1.GetGenericTypeDefinition() == tuple.Item2.GetGenericTypeDefinition();
									return match;
								} );
				return result;
			}
		}
	}
}