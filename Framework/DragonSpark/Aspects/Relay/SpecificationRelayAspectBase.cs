namespace DragonSpark.Aspects.Relay
{
	public abstract class SpecificationRelayAspectBase : ApplyRelayAspectBase, ISpecificationRelay
	{
		readonly ISpecificationRelay relay;

		protected SpecificationRelayAspectBase( ISupportDefinition definition ) : base( definition ) {}

		protected SpecificationRelayAspectBase( ISpecificationRelay relay, ISupportDefinition definition ) : base( definition )
		{
			this.relay = relay;
		}

		public bool IsSatisfiedBy( object parameter ) => relay.IsSatisfiedBy( parameter );
	}
}