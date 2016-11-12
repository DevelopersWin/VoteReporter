using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Exceptions
{
	sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base(
			MethodAspectSelector<Aspect>.Default,

			GenericCommandTypeDefinition.Default,
			ParameterizedSourceTypeDefinition.Default,
			GenericSpecificationTypeDefinition.Default
		) {}
	}
}