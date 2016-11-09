using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplySpecificationRelayDefinition : ApplyRelayAspectBuildDefinition<ISpecificationRelay, ApplySpecificationRelay>
	{
		public static ApplySpecificationRelayDefinition Default { get; } = new ApplySpecificationRelayDefinition();
		ApplySpecificationRelayDefinition() : base( 
			GeneralizedSpecificationTypeDefinition.Default.ReferencedType, GenericSpecificationTypeDefinition.Default.ReferencedType, 
			typeof(SpecificationRelayAdapter<>),
			new MethodAspectSelector<Specification>( GeneralizedSpecificationTypeDefinition.Default.Method )
		) {}
	}
}