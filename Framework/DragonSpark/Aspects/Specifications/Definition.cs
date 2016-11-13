using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Definition : IntroducedAspectBuildDefinition<IntroduceSpecification, Aspect>
	{
		public Definition( params object[] parameters ) 
			: base( parameters.ToImmutableArray(), SpecificationTypeDefinition.Default ) {}
	}
}