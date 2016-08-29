using System;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class SpecificationParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		readonly Func<TParameter, bool> specification;

		public SpecificationParameterizedSource( Func<TParameter, bool> specification, Func<TParameter, TResult> source ) : base( source )
		{
			this.specification = specification;
		}

		public override TResult Get( TParameter parameter ) => specification( parameter ) ? base.Get( parameter ) : default(TResult);
	}
}