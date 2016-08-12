using System;

namespace DragonSpark.Runtime.Sources
{
	public class DelegatedParameterizedSource<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedParameterizedSource( Func<TParameter, TResult> source )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source( parameter );
	}
}