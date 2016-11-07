using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationDescriptor : RelayMethodDefinition<ISpecificationRelay, ApplySpecificationRelay>
	{
		public static SpecificationDescriptor Default { get; } = new SpecificationDescriptor();
		SpecificationDescriptor() : base( 
			GeneralizedSpecificationTypeDefinition.Default.ReferencedType, GenericSpecificationTypeDefinition.Default.ReferencedType, 
			typeof(SpecificationRelayAdapter<>),
			new MethodBasedAspectInstanceLocator<Specification>( GeneralizedSpecificationTypeDefinition.Default.Method )
		) {}
	}
}