using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( IntroducedAspectSelector<IntroduceSpecification, Aspect>.Default, new IntroducedTypeDefinition( SpecificationTypeDefinition.Default ) ) {}
	}
}