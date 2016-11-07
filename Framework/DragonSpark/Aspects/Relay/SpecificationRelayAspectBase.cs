namespace DragonSpark.Aspects.Relay
{
	public abstract class SpecificationRelayAspectBase : RelayAspectBase
	{
		protected SpecificationRelayAspectBase( IRelayMethodDefinition definition ) : base( definition ) {}

		protected SpecificationRelayAspectBase( ISpecificationRelay relay ) : base( relay ) {}
	}
}