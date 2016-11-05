using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Exceptions
{
	sealed class Support : SupportDefinition<Aspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( GenericCommandTypeDefinition.Default, ParameterizedSourceTypeDefinition.Default, GenericSpecificationTypeDefinition.Default ) {}
	}
}