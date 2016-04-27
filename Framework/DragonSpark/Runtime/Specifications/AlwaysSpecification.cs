namespace DragonSpark.Runtime.Specifications
{
	public class AlwaysSpecification<T> : BoxedSpecification<T>
	{
		public static AlwaysSpecification<T> Instance { get; } = new AlwaysSpecification<T>();

		AlwaysSpecification() : base( new FixedSpecification( true ) ) {}
	}
}