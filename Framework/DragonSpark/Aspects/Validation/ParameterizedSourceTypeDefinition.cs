using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Validation
{
	sealed class ParameterizedSourceTypeDefinition : ValidatedTypeDefinition
	{
		public static ParameterizedSourceTypeDefinition Default { get; } = new ParameterizedSourceTypeDefinition();
		ParameterizedSourceTypeDefinition() : base( Definitions.ParameterizedSourceTypeDefinition.Default.PrimaryMethod ) {}
	}
}