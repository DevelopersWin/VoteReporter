using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Relay
{
	public abstract class SpecificationRelayAspectBase : ApplyRelayAspectBase, ISpecificationRelay
	{
		readonly ISpecificationRelay relay;

		protected SpecificationRelayAspectBase( IParameterizedSource<IAspect> definition ) : base( definition ) {}

		protected SpecificationRelayAspectBase( ISpecificationRelay relay, IParameterizedSource<IAspect> definition ) : base( definition )
		{
			this.relay = relay;
		}

		public bool IsSatisfiedBy( object parameter ) => relay.IsSatisfiedBy( parameter );
	}
}