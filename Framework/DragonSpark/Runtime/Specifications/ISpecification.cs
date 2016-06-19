using System.Collections.Generic;

namespace DragonSpark.Runtime.Specifications
{
	public interface ISpecification
	{
		bool IsSatisfiedBy( object parameter );
	}

	public interface ISpecification<in T> : ISpecification
	{
		bool IsSatisfiedBy( T parameter );
	}

	public delegate bool Specification<in T>( T parameter );
}