using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class ParameterizedSourceAspectSelector : AspectSelector<IntroduceGeneralizedParameterizedSource>
	{
		public static ParameterizedSourceAspectSelector Default { get; } = new ParameterizedSourceAspectSelector();
		ParameterizedSourceAspectSelector() : base( ParameterizedSourceTypeDefinition.Default.ReferencedType, GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType ) {}
	}
}