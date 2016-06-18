namespace DragonSpark.Runtime.Specifications
{
	public abstract class SpecificationWithContextBase<T> : SpecificationWithContextBase<T, T>
	{
		protected SpecificationWithContextBase( T context ) : base( context ) {}
	}

	public abstract class SpecificationWithContextBase<TParameter, TContext> : GuardedSpecificationBase<TParameter>
	{
		protected SpecificationWithContextBase( TContext context )
		{
			Context = context;
		}

		protected TContext Context { get; }
	}
}