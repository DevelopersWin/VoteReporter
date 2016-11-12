using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class ParameterizedSourceAspectDefinition : AspectDefinition<IntroduceGeneralizedParameterizedSource>
	{
		public static ParameterizedSourceAspectDefinition Default { get; } = new ParameterizedSourceAspectDefinition();
		ParameterizedSourceAspectDefinition() : base( ParameterizedSourceTypeDefinition.Default, GeneralizedParameterizedSourceTypeDefinition.Default ) {}
	}
}