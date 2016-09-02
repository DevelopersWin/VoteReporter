using DragonSpark.Specifications;
using System;

namespace DragonSpark.Sources.Parameterized
{
	public class SpecificationParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>, ISpecification<TParameter>
	{
		readonly ISpecification<TParameter> specification;

		public SpecificationParameterizedSource( ISpecification<TParameter> specification, Func<TParameter, TResult> source ) : base( source )
		{
			this.specification = specification;
		}

		public override TResult Get( TParameter parameter )
		{
			var isSatisfiedBy = specification.IsSatisfiedBy( parameter );
			var result = isSatisfiedBy ? base.Get( parameter ) : default(TResult);
			return result;
		}

		public bool IsSatisfiedBy( TParameter parameter ) => specification.IsSatisfiedBy( parameter );
		// bool ISpecification.IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( parameter );
	}
}