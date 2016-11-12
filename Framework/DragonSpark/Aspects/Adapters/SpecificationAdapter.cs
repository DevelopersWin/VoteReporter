using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Adapters
{
	public class SpecificationAdapter<T> : DelegatedAdapter<T, bool>, ISpecification<object>, ISpecificationRelayAdapter
	{
		public SpecificationAdapter( ISpecification<T> specification ) : base( specification.IsSatisfiedBy ) {}

		public bool IsSatisfiedBy( object parameter ) => GetGeneral( parameter );
	}
}