using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class SpecificationTypeDefinition : SpecificationTypeDefinitionBase
	{
		public static SpecificationTypeDefinition Default { get; } = new SpecificationTypeDefinition();
		SpecificationTypeDefinition() : base( typeof(ISpecification<>) ) {}
	}
}