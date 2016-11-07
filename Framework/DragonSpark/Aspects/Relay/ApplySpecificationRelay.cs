using JetBrains.Annotations;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplySpecificationRelay : SpecificationRelayAspectBase
	{
		public ApplySpecificationRelay()  : base( SpecificationDescriptor.Default ) {}

		[UsedImplicitly]
		public ApplySpecificationRelay( ISpecificationRelay relay ) : base( relay ) {}
	}
}