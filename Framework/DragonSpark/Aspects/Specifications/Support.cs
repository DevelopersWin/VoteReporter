using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Support : AspectBuildDefinition<Aspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( GenericSpecificationTypeDefinition.Default ) {}
	}
}