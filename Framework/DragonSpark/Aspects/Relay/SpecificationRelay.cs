using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationRelay : RelayMethodBase
	{
		public SpecificationRelay() : base( SourceCoercer<ISpecificationRelayAdapter>.Default.Get ) {}
	}
}