using System;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	public class DelegatedValidatedSource<TParameter, TResult> : ValidatedParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> inner;

		public DelegatedValidatedSource( Func<TParameter, TResult> inner ) : this( inner, Specifications<TParameter>.Always ) {}

		public DelegatedValidatedSource( Func<TParameter, TResult> inner, ISpecification<TParameter> specification ) : this( inner, Defaults<TParameter>.Coercer, specification ) {}

		public DelegatedValidatedSource( Func<TParameter, TResult> inner, Coerce<TParameter> coercer, ISpecification<TParameter> specification ) : base( coercer, specification )
		{
			this.inner = inner;
		}

		public override TResult Get( TParameter parameter ) => inner( parameter );
	}
}