using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Implementations
{
	sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( ParameterizedSelectorAspectSelector.Default, SpecificationAspectSelector.Default ) {}
	}
}