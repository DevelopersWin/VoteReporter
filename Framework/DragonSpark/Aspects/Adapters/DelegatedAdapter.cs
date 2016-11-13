using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Adapters
{
	public class DelegatedAdapter<TParameter, TResult> : AdapterBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedAdapter( IParameterizedSource<TParameter, TResult> source ) : this( DefaultCoercer, source ) {}
		public DelegatedAdapter( IParameterizedSource<object, TParameter> coercer, IParameterizedSource<TParameter, TResult> source ) : this( coercer, source.ToDelegate() ) {}

		public DelegatedAdapter( Func<TParameter, TResult> source ) : this( DefaultCoercer, source ) {}

		public DelegatedAdapter( IParameterizedSource<object, TParameter> coercer, Func<TParameter, TResult> source ) : base( coercer )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source( parameter );
	}
}