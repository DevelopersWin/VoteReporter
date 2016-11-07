using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Support<T> : Definition<T> where T : AlterationAspectBase
	{
		public static Support<T> Default { get; } = new Support<T>();
		Support() : base( GenericCommandCoreTypeDefinition.Default, ParameterizedSourceTypeDefinition.Default ) {}
	}
}