namespace DragonSpark.Runtime.Specifications
{
	public class FixedSpecification : SpecificationBase<object>
	{
		readonly bool satisfied;

		public FixedSpecification( bool satisfied )
		{
			this.satisfied = satisfied;
		}

		protected override bool Verify( object parameter ) => satisfied;
	}
}