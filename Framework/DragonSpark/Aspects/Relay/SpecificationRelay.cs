using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationRelay : RelayAspectBase
	{
		public SpecificationRelay() : base( SourceCoercer<ISpecificationRelayAdapter>.Default.Get ) {}
	}
}