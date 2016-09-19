using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Support : SupportDefinition<SpecificationAspect>
	{
		public static Support Default { get; } = new Support();
		Support() : base( SpecificationDefinition.Default ) {}
	}


}