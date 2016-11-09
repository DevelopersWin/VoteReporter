using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplySpecificationRelayDefinition : AspectBuildDefinition<ISpecificationRelay, ApplySpecificationRelay>
	{
		public static ApplySpecificationRelayDefinition Default { get; } = new ApplySpecificationRelayDefinition();
		ApplySpecificationRelayDefinition() : base( 
			GeneralizedSpecificationTypeDefinition.Default.ReferencedType, GenericSpecificationTypeDefinition.Default.ReferencedType, 
			typeof(SpecificationRelayAdapter<>),
			new MethodAspectSelector<Specification>( GeneralizedSpecificationTypeDefinition.Default.Method )
		) {}
	}
}