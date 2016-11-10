using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Adapters
{
	public class DelegatedAdapter<TParameter, TResult> : AdapterBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedAdapter( IParameterizedSource<TParameter, TResult> source ) : this( source.ToDelegate() ) {}

		public DelegatedAdapter( Func<TParameter, TResult> source )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source( parameter );
	}
}