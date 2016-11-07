namespace DragonSpark.Aspects.Relay
{
	public abstract class SpecificationRelayAspectBase : RelayAspectBase
	{
		protected SpecificationRelayAspectBase( IRelayMethodAspectBuildDefinition aspectBuildDefinition ) : base( aspectBuildDefinition ) {}

		protected SpecificationRelayAspectBase( ISpecificationRelay relay ) : base( relay ) {}
	}
}