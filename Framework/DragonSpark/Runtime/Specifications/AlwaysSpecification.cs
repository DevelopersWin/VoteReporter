namespace DragonSpark.Runtime.Specifications
{
	public class Specifications : Specifications<object> {}

	public class Specifications<T>
	{
		public static ISpecification<T> Assigned { get; } = AssignedSpecification<T>.Instance;
		
		public static ISpecification<T> Always { get; } = new FixedSpecification<T>( true );

		public static ISpecification<T> Never { get; } = new FixedSpecification<T>( false );

		// public static ISpecification<T> IsInstanceOf { get; } = IsInstanceOfSpecification<T>.Instance.Cast<T>();
	}

	/*public class AlwaysSpecification<T> : FixedSpecification<T>
	{
		public static AlwaysSpecification<T> Instance { get; } = new AlwaysSpecification<T>();

		public AlwaysSpecification() : base( true ) {}
	}*/
}