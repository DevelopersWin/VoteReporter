using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class SpecificationAspectDefinition : AspectDefinition<IntroduceGeneralizedSpecification>
	{
		public static SpecificationAspectDefinition Default { get; } = new SpecificationAspectDefinition();
		SpecificationAspectDefinition() : base( 
			GenericSpecificationTypeDefinition.Default, 
			
			GeneralizedSpecificationTypeDefinition.Default, 
			CommandTypeDefinition.Default ) {}
	}
}