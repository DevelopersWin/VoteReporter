namespace DragonSpark.Specifications
{
	/*public interface ISpecification
	{
		bool IsSatisfiedBy( object parameter );
	}*/

	public interface ISpecification<in T>
	{
		bool IsSatisfiedBy( T parameter );
	}
}