namespace DragonSpark.Runtime.Specifications
{
	public class FixedSpecification : FixedSpecification<object>
	{
		public FixedSpecification( bool satisfied ) : base( satisfied ) {}
	}

	public class FixedSpecification<T> : SpecificationBase<T>
	{
		readonly bool satisfied;

		public FixedSpecification( bool satisfied )
		{
			this.satisfied = satisfied;
		}

		public override bool IsSatisfiedBy( T parameter ) => satisfied;
	}
}