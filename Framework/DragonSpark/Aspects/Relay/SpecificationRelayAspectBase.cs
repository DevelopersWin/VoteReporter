namespace DragonSpark.Aspects.Relay
{
	public abstract class SpecificationRelayAspectBase : RelayAspectBase
	{
		protected SpecificationRelayAspectBase( IApplyRelayAspectBuildDefinition definition ) : base( definition ) {}

		protected SpecificationRelayAspectBase( ISpecificationRelay relay ) : base( relay.Get ) {}
	}
}