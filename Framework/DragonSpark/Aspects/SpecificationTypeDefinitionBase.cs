using DragonSpark.Aspects.Build;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Aspects
{
	public abstract class SpecificationTypeDefinitionBase : TypeDefinitionWithPrimaryMethodBase
	{
		protected SpecificationTypeDefinitionBase( Type specificationType ) : base( new Methods( specificationType, nameof(ISpecification<object>.IsSatisfiedBy) ) ) {}
	}
}