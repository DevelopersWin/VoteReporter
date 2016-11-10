using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Adapters
{
	public class SpecificationAdapter<T> : DelegatedAdapter<T, bool>, ISpecificationRelayAdapter
	{
		public SpecificationAdapter( ISpecification<T> specification ) : base( specification.IsSatisfiedBy ) {}
	}
}