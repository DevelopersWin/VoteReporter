using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class ParameterizedSourceAspectSource : AspectSource<IntroduceGeneralizedParameterizedSource>
	{
		public static ParameterizedSourceAspectSource Default { get; } = new ParameterizedSourceAspectSource();
		ParameterizedSourceAspectSource() : base( ParameterizedSourceTypeDefinition.Default.ReferencedType, GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType ) {}
	}
}