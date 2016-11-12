namespace DragonSpark.Aspects.Adapters
{
	public sealed class DefaultSpecificationImplementation<T> : DefaultSpecificationImplementationBase<T>
	{
		public DefaultSpecificationImplementation( ISpecificationAdapter specification ) : base( specification ) {}
	}

	public sealed class DefaultSpecificationImplementation : DefaultSpecificationImplementationBase<object>
	{
		public DefaultSpecificationImplementation( ISpecificationAdapter specification ) : base( specification ) {}
	}
}