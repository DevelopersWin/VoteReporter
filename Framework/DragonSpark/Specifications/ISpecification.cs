namespace DragonSpark.Specifications
{
	public interface ISpecification
	{
		bool IsSatisfiedBy( object parameter );
	}

	public interface ISpecification<in T> // : ISpecification
	{
		bool IsSatisfiedBy( T parameter );
	}
}