using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Adapters
{
	public class DelegatedAdapter<TParameter, TResult> : AdapterBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedAdapter( IParameterizedSource<TParameter, TResult> source ) : this( source, Coercer ) {}
		public DelegatedAdapter( IParameterizedSource<TParameter, TResult> source, IParameterizedSource<object, TParameter> coercer ) : this( source.ToDelegate(), coercer ) {}

		public DelegatedAdapter( Func<TParameter, TResult> source ) : this( source, Coercer ) {}

		public DelegatedAdapter( Func<TParameter, TResult> source, IParameterizedSource<object, TParameter> coercer ) : base( coercer )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source( parameter );
	}
}