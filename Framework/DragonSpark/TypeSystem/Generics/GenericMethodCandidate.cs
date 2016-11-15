using System;
using System.Collections.Immutable;

namespace DragonSpark.TypeSystem.Generics
{
	public struct GenericMethodCandidate<T>
	{
		public GenericMethodCandidate( T @delegate, Func<ImmutableArray<Type>, bool> specification )
		{
			Delegate = @delegate;
			Specification = specification;
		}

		public T Delegate { get; }
		public Func<ImmutableArray<Type>, bool> Specification { get; }
	}
}