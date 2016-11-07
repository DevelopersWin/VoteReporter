using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ParameterizedSourceRelayAdapter<TParameter, TResult> : DelegatedInvocation<TParameter, TResult>, IParameterizedSourceRelay
	{
		public ParameterizedSourceRelayAdapter( IParameterizedSource<TParameter, TResult> source ) : base( source ) {}
	}
}