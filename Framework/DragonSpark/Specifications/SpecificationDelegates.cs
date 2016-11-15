using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Specifications
{
	public sealed class SpecificationDelegates<T> : Cache<ISpecification<T>, Func<T, bool>>
	{
		public static SpecificationDelegates<T> Default { get; } = new SpecificationDelegates<T>();
		SpecificationDelegates() : base( specification => specification.IsSatisfiedBy ) {}
	}
}