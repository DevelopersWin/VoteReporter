namespace DragonSpark.Specifications
{
	public abstract class SpecificationBase<T> : ISpecification<T>
	{
		public abstract bool IsSatisfiedBy( T parameter );

		// bool ISpecification.IsSatisfiedBy( [Optional]object parameter ) => parameter is T && IsSatisfiedBy( (T)parameter );
	}
}