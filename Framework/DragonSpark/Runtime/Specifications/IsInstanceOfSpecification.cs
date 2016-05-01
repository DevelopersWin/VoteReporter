namespace DragonSpark.Runtime.Specifications
{
	public class IsInstanceOfSpecification<T> : ISpecification
	{
		public static IsInstanceOfSpecification<T> Instance { get; } = new IsInstanceOfSpecification<T>();

		public bool IsSatisfiedBy( object parameter ) => parameter is T;
	}
}