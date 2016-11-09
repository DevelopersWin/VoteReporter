using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Relay
{
	public sealed class Specification : MethodAspectBase
	{
		public Specification() : base( o => o is ISpecificationRelay ) {}
	}
}