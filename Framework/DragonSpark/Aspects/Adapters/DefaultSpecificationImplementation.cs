using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class DefaultSpecificationImplementation<T> : SpecificationBase<T>
	{
		readonly ISpecificationAdapter specification;

		public DefaultSpecificationImplementation( ISpecificationAdapter specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => (bool)specification.Get( parameter );
	}
}