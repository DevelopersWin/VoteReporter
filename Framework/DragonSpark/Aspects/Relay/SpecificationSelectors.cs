using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationSelectors : AspectSelectors<ISpecificationRelayAdapter, ApplySpecificationRelay>
	{
		public static SpecificationSelectors Default { get; } = new SpecificationSelectors();
		SpecificationSelectors() : base( 
			SpecificationTypeDefinition.Default.ReferencedType, 
			typeof(SpecificationAdapter<>),
			new MethodAspectDefinition<SpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default.PrimaryMethod )
		) {}
	}
}