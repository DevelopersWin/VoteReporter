using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class ParameterizedSourceTypeDefinition : TypeDefinitionWithPrimaryMethodBase
	{
		public static ParameterizedSourceTypeDefinition Default { get; } = new ParameterizedSourceTypeDefinition();
		ParameterizedSourceTypeDefinition() : base( new Methods( typeof(IParameterizedSource<,>), nameof(ISource.Get) ) ) {}
	}
}