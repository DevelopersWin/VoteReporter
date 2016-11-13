using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Sources.Coercion
{
	public class ResultCoercionSource<TParameter, TResult, TTo> : DelegatedParameterizedSource<TParameter, TResult>, IParameterizedSource<TParameter, TTo>
	{
		readonly Func<TResult, TTo> coercer;

		public ResultCoercionSource( Func<TParameter, TResult> source, Func<TResult, TTo> coercer ) : base( source )
		{
			this.coercer = coercer;
		}

		public new TTo Get( TParameter parameter )
		{
			var prior = base.Get( parameter );
			var result = prior != null ? coercer( prior ) : default(TTo);
			return result;
		}
	}
}