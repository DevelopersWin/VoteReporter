namespace DragonSpark.Specifications
{
	public class Common<T>
	{
		public static ISpecification<T> Assigned { get; } = AssignedSpecification<T>.Default;
		
		public static ISpecification<T> Always { get; } = new SuppliedSpecification<T>( true );

		public static ISpecification<T> Never { get; } = new SuppliedSpecification<T>( false );
	}
}