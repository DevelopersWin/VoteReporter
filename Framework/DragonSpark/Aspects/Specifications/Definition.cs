using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( AspectSelection<IntroduceSpecification, Aspect>.Default, GenericSpecificationTypeDefinition.Default ) {}
	}
}