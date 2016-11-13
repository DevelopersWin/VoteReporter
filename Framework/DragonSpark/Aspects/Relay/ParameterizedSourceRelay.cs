using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ParameterizedSourceRelay : RelayMethodBase
	{
		public ParameterizedSourceRelay() : base( AdapterInvocation<IParameterizedSourceAdapter>.Default ) {}
	}
}