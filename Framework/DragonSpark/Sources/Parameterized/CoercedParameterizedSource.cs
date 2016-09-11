using DragonSpark.Extensions;
using System;

namespace DragonSpark.Sources.Parameterized
{
	public class CoercedParameterizedSource<TParameter, TResult> : CoercedParameterizedSource<object, TParameter, TResult>
	{
		public CoercedParameterizedSource( Func<TParameter, TResult> source ) : this( Defaults<TParameter>.Coercer, source ) {}
		public CoercedParameterizedSource( Coerce<TParameter> coercer, Func<TParameter, TResult> source ) : base( new Func<object, TParameter>( coercer ), source ) {}
	}

	public class CoercedParameterizedSource<TFrom, TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>, IParameterizedSource<TFrom, TResult>
	{
		readonly Func<TFrom, TParameter> coercer;

		public CoercedParameterizedSource( Func<TFrom, TParameter> coercer, Func<TParameter, TResult> source ) : base( source )
		{
			this.coercer = coercer;
		}

		public TResult Get( TFrom parameter )
		{
			var to = coercer( parameter );
			var result = to.IsAssignedOrValue() ? Get( to ) : default(TResult);
			return result;
		}
	}
}