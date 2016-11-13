using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Adapters
{
	public class SpecificationAdapter<T> : DelegatedAdapter<T, bool>, ISpecificationRelayAdapter
	{
		public SpecificationAdapter( ISpecification<T> implementation ) : base( implementation.IsSatisfiedBy ) {}

		// public bool IsSatisfiedBy( object parameter ) => GetGeneral<bool>( parameter );
	}
}