using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects
{
	public sealed class ParameterizedSourceDefinition : ValidatedComponentDefinition
	{
		public static ParameterizedSourceDefinition Default { get; } = new ParameterizedSourceDefinition();
		ParameterizedSourceDefinition() : base( typeof(IParameterizedSource<,>), nameof(ISource.Get) ) {}
	}
}