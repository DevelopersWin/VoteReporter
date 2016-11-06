using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationDescriptor : SupportedMethodsDefinition<SpecificationRelayAspect>
	{
		public static SpecificationDescriptor Default { get; } = new SpecificationDescriptor();
		SpecificationDescriptor() : base( 
			GeneralizedSpecificationTypeDefinition.Default.DeclaringType, GenericSpecificationTypeDefinition.Default.DeclaringType, 
			typeof(SpecificationRelay<>), typeof(ISpecificationRelay),
			new MethodBasedAspectInstanceLocator<Specification>( GeneralizedSpecificationTypeDefinition.Default.Method )
		) {}
	}
}