using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class SpecificationAspectSelector : AspectSelector<IntroduceGeneralizedSpecification>
	{
		public static SpecificationAspectSelector Default { get; } = new SpecificationAspectSelector();
		SpecificationAspectSelector() : base( 
			GenericSpecificationTypeDefinition.Default.ReferencedType, 
			GeneralizedSpecificationTypeDefinition.Default.ReferencedType, 
			CommandTypeDefinition.Default.ReferencedType ) {}
	}
}