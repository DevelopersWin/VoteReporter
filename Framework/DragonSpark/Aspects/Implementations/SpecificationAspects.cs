using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class SpecificationAspects : Aspects<IntroduceGeneralizedSpecification>
	{
		public static SpecificationAspects Default { get; } = new SpecificationAspects();
		SpecificationAspects() : base( 
			SpecificationTypeDefinition.Default, 
			
			GeneralizedSpecificationTypeDefinition.Default, 
			CommandTypeDefinition.Default ) {}
	}
}