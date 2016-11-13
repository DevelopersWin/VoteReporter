using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class ParameterizedSourceAspects : Aspects<IntroduceGeneralizedParameterizedSource>
	{
		public static ParameterizedSourceAspects Default { get; } = new ParameterizedSourceAspects();
		ParameterizedSourceAspects() : base( ParameterizedSourceTypeDefinition.Default, GeneralizedParameterizedSourceTypeDefinition.Default ) {}
	}
}