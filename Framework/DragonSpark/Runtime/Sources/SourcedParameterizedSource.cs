using System;

namespace DragonSpark.Runtime.Sources
{
	public class SourcedParameterizedSource<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly ISource<Func<TParameter, TResult>> source;

		public SourcedParameterizedSource( ISource<Func<TParameter, TResult>> source )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source.Get()( parameter );
	}
}