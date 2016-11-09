namespace DragonSpark.Aspects.Implementations
{
	public sealed class SpecificationAspectSelector : AspectSelector<GeneralizedSpecificationAspect>
	{
		public static SpecificationAspectSelector Default { get; } = new SpecificationAspectSelector();
		SpecificationAspectSelector() : base( 
			GenericSpecificationTypeDefinition.Default.ReferencedType, 
			GeneralizedSpecificationTypeDefinition.Default.ReferencedType, 
			CommandTypeDefinition.Default.ReferencedType ) {}
	}
}