using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Support : Definition<Aspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( GenericSpecificationTypeDefinition.Default ) {}
	}
}