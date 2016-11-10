using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class ParameterizedSelectorAspectSelector : AspectSelector<IntroduceGeneralizedParameterizedSource>
	{
		public static ParameterizedSelectorAspectSelector Default { get; } = new ParameterizedSelectorAspectSelector();
		ParameterizedSelectorAspectSelector() : base( ParameterizedSourceTypeDefinition.Default.ReferencedType, GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType ) {}
	}
}