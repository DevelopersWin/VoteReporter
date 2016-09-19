using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Support : SupportDefinition<Aspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( CommandDefinition.Default, GenericCommandDefinition.Default, ParameterizedSourceDefinition.Default ) {}
	}
}