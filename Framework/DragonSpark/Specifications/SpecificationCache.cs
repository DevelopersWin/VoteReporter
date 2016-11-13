using System;

namespace DragonSpark.Specifications
{
	public class SpecificationCache<T> : SpecificationCache<T, T> where T : class
	{
		public SpecificationCache( Func<T, ISpecification<T>> create ) : base( create ) {}
	}
}