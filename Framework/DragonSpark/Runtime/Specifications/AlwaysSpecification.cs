namespace DragonSpark.Runtime.Specifications
{
	public class AlwaysSpecification : AlwaysSpecification<object>
	{
		public new static AlwaysSpecification Instance { get; } = new AlwaysSpecification();
	}

	public class AlwaysSpecification<T> : BoxedSpecification<T>
	{
		public static AlwaysSpecification<T> Instance { get; } = new AlwaysSpecification<T>();

		protected AlwaysSpecification() : base( new FixedSpecification( true ) ) {}
	}
}