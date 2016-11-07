using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ParameterizedSourceRelayAdapter<TParameter, TResult> : InvocationBase<TParameter, TResult>
	{
		readonly IParameterizedSource<TParameter, TResult> source;

		public ParameterizedSourceRelayAdapter( IParameterizedSource<TParameter, TResult> source )
		{
			this.source = source;
		}

		public override TResult Invoke( TParameter parameter ) => source.Get( parameter );
	}
}