using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class GeneralizedParameterizedSourceTypeDefinition : TypeDefinitionWithPrimaryMethodBase
	{
		public static GeneralizedParameterizedSourceTypeDefinition Default { get; } = new GeneralizedParameterizedSourceTypeDefinition();
		GeneralizedParameterizedSourceTypeDefinition() : base( new Methods( typeof(IParameterizedSource<object, object>), nameof(IParameterizedSource<object, object>.Get) ) ) {}
	}
}