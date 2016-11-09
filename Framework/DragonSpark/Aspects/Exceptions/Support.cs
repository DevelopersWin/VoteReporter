using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Exceptions
{
	sealed class Support : AspectBuildDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : base(
			MethodAspectLocatorFactory<Aspect>.Default.GetFixed(
				GenericCommandTypeDefinition.Default,
				ParameterizedSourceTypeDefinition.Default,
				GenericSpecificationTypeDefinition.Default
			)
		) {}
	}
}