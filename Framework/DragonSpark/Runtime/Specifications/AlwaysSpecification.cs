namespace DragonSpark.Runtime.Specifications
{
	public class Specifications<T>
	{
		public static ISpecification<T> NotNull { get; } = NotNullSpecification.Instance.Box<T>();
		
		public static ISpecification<T> Always { get; } = AlwaysSpecification.Instance.Box<T>();

		public static ISpecification<T> IsInstanceOf { get; } = IsInstanceOfSpecification<T>.Instance.Box<T>();
	}

	public class AlwaysSpecification : FixedSpecification
	{
		public static AlwaysSpecification Instance { get; } = new AlwaysSpecification();

		public AlwaysSpecification() : base( true ) {}
	}
}