namespace DragonSpark.Specifications
{
	public class CastingSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification specification;

		public CastingSpecification( ISpecification specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( parameter );
	}
}