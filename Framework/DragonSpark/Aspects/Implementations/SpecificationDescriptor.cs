namespace DragonSpark.Aspects.Implementations
{
	public sealed class SpecificationDescriptor : Descriptor<GeneralizedSpecificationAspect>
	{
		public static SpecificationDescriptor Default { get; } = new SpecificationDescriptor();
		SpecificationDescriptor() : base( 
			GenericSpecificationTypeDefinition.Default.ReferencedType, 
			GeneralizedSpecificationTypeDefinition.Default.ReferencedType, 
			CommandTypeDefinition.Default.ReferencedType ) {}
	}
}