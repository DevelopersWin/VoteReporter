using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem.Generics
{
	public sealed class MethodContext<T> : ArgumentCache<Type[], T> where T : class 
	{
		public MethodContext( ImmutableArray<GenericMethodCandidate<T>> candidates ) : this( new Factory( candidates ).Get ) {}
		MethodContext( Func<Type[], T> resultSelector ) : base( resultSelector ) {}

		sealed class Factory : ParameterizedSourceBase<Type[], T>
		{
			readonly ImmutableArray<GenericMethodCandidate<T>> candidates;

			public Factory( ImmutableArray<GenericMethodCandidate<T>> candidates )
			{
				this.candidates = candidates;
			}

			public override T Get( Type[] parameter ) => candidates.Introduce( parameter, tuple => tuple.Item1.Specification( tuple.Item2 ), tuple => tuple.Item1.Delegate ).Single();
		}
	}
}