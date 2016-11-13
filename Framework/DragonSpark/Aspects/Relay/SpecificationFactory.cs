using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationFactory : AspectFactory<ISpecificationRelayAdapter, ApplySpecificationRelay>
	{
		public static SpecificationFactory Default { get; } = new SpecificationFactory();
		SpecificationFactory() : base( SpecificationTypeDefinition.Default.ReferencedType, typeof(SpecificationAdapter<>) ) {}
	}
}