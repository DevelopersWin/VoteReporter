using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class SpecificationAspectSource : AspectSource<IntroduceGeneralizedSpecification>
	{
		public static SpecificationAspectSource Default { get; } = new SpecificationAspectSource();
		SpecificationAspectSource() : base( 
			GenericSpecificationTypeDefinition.Default.ReferencedType, 
			GeneralizedSpecificationTypeDefinition.Default.ReferencedType, 
			CommandTypeDefinition.Default.ReferencedType ) {}
	}
}