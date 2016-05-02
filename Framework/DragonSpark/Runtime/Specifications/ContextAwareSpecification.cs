namespace DragonSpark.Runtime.Specifications
{
	public abstract class SpecificationWithContextBase<TParameter> : GuardedSpecificationBase<TParameter>
	{
		protected SpecificationWithContextBase( TParameter context )
		{
			Context = context;
		}

		protected TParameter Context { get; }
	}
}