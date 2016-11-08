namespace DragonSpark.Specifications
{
	public abstract class SpecificationWithContextBase<TContext, TParameter> : SpecificationBase<TParameter>
	{
		protected SpecificationWithContextBase( TContext context )
		{
			Context = context;
		}

		protected TContext Context { get; }
	}
}