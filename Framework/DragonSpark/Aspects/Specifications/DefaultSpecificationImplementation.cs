using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Specifications
{
	sealed class DefaultSpecificationImplementation<T> : Adapters.SpecificationAdapter<T>, ISpecification<T>
	{
		public DefaultSpecificationImplementation( ISpecification<T> specification ) : base( specification ) {}

		public bool IsSatisfiedBy( T parameter ) => Get( parameter );
	}
}