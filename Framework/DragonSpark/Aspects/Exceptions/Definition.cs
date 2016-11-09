using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Exceptions
{
	sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base(
			MethodAspectLocatorFactory<Aspect>.Default.GetFixed(
				GenericCommandTypeDefinition.Default,
				ParameterizedSourceTypeDefinition.Default,
				GenericSpecificationTypeDefinition.Default
			)
		) {}
	}
}