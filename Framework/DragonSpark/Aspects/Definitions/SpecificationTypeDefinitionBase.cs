using System;
using DragonSpark.Aspects.Build;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Definitions
{
	public abstract class SpecificationTypeDefinitionBase : TypeDefinitionWithPrimaryMethodBase
	{
		protected SpecificationTypeDefinitionBase( Type specificationType ) : base( new Methods( specificationType, nameof(ISpecification<object>.IsSatisfiedBy) ) ) {}
	}
}