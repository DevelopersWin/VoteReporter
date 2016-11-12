using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Adapters
{
	public abstract class DefaultSpecificationImplementationBase<T> : SpecificationBase<T>
	{
		readonly ISpecificationAdapter specification;

		protected DefaultSpecificationImplementationBase( ISpecificationAdapter specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => (bool)specification.Get( parameter );
	}
}