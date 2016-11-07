using DragonSpark.Sources.Coercion;

namespace DragonSpark.Aspects.Relay
{
	public sealed class Specification : MethodAspectBase
	{
		public Specification() : base( CastCoercer<ISpecificationRelay>.Default.Get ) {}
	}
}