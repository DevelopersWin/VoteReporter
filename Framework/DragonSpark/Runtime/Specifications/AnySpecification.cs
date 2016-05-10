using System.Linq;

namespace DragonSpark.Runtime.Specifications
{
	public class AnySpecification : AnySpecification<object>
	{
		public AnySpecification( params ISpecification<object>[] specifications ) : base( specifications ) {}
	}

	public class AnySpecification<T> : CompositeSpecification<T>
	{
		public AnySpecification( params ISpecification<T>[] specifications ) : base( specifications.Any ) {}
	}
}