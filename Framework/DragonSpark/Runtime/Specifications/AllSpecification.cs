using System.Linq;

namespace DragonSpark.Runtime.Specifications
{
	public class AllSpecification : AllSpecification<object>
	{
		public AllSpecification( params ISpecification<object>[] specifications ) : base( specifications ) {}
	}

	public class AllSpecification<T> : CompositeSpecification<T>
	{
		public AllSpecification( params ISpecification<T>[] specifications ) : base( specifications.All ) {}
	}
}