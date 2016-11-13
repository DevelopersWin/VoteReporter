using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationRelay : RelayMethodBase
	{
		public SpecificationRelay() : base( AdapterInvocation<ISpecificationRelayAdapter>.Default ) {}
	}
}