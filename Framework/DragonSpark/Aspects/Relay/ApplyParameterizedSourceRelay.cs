using JetBrains.Annotations;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplyParameterizedSourceRelay : RelayAspectBase
	{
		public ApplyParameterizedSourceRelay() : base( ApplySourceRelayDefinition.Default ) {}

		[UsedImplicitly]
		public ApplyParameterizedSourceRelay( IParameterizedSourceRelay relay ) : base( relay.Get ) {}
	}
}