using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplySpecificationRelayDefinition : AspectBuildDefinition<ISpecificationRelayAdapter, ApplySpecificationRelay>
	{
		public static ApplySpecificationRelayDefinition Default { get; } = new ApplySpecificationRelayDefinition();
		ApplySpecificationRelayDefinition() : base( 
			GenericSpecificationTypeDefinition.Default.ReferencedType, 
			typeof(SpecificationAdapter<>),
			new MethodAspectSelector<SpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default.Method )
		) {}
	}
}