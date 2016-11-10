using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( 
			AspectLocatorFactory<IntroduceSpecification, Aspect>
				.Default
				.GetFixed( GenericSpecificationTypeDefinition.Default )
		) {}
	}
}