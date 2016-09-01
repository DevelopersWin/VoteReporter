using System;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	public class SpecificationParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>, IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly Func<TParameter, bool> specification;

		public SpecificationParameterizedSource( Func<TParameter, bool> specification, Func<TParameter, TResult> source ) : base( source )
		{
			this.specification = specification;
		}

		public override TResult Get( TParameter parameter ) => specification( parameter ) ? base.Get( parameter ) : default(TResult);

		public bool IsSatisfiedBy( TParameter parameter ) => specification( parameter );
		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter is TParameter && specification( (TParameter)parameter );
	}
}