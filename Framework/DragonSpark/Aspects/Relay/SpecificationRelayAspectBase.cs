using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Relay
{
	public abstract class SpecificationRelayAspectBase : RelayAspectBase
	{
		protected SpecificationRelayAspectBase( IAspectBuildDefinition definition ) : base( definition ) {}

		protected SpecificationRelayAspectBase( ISpecificationRelay relay ) : base( relay.Get ) {}
	}
}