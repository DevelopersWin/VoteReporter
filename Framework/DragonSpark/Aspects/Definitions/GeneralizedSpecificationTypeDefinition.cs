using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class GeneralizedSpecificationTypeDefinition : SpecificationTypeDefinitionBase
	{
		public static GeneralizedSpecificationTypeDefinition Default { get; } = new GeneralizedSpecificationTypeDefinition();
		GeneralizedSpecificationTypeDefinition() : base( typeof(ISpecification<object>) ) {}
	}
}