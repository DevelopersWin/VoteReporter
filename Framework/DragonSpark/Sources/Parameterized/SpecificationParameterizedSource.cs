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
			var validated = Validate( parameter );
			var result = validated ? base.Get( parameter ) : default(TResult);
			return result;
		}

		protected virtual bool Validate( TParameter parameter ) => IsSatisfiedBy( parameter );

		public bool IsSatisfiedBy( TParameter parameter ) => specification.IsSatisfiedBy( parameter );
	}
}