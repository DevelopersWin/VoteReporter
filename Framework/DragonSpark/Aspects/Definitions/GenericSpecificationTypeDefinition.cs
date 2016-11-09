using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class GenericSpecificationTypeDefinition : SpecificationTypeDefinitionBase
	{
		public static GenericSpecificationTypeDefinition Default { get; } = new GenericSpecificationTypeDefinition();
		GenericSpecificationTypeDefinition() : base( typeof(ISpecification<>) ) {}
	}
}