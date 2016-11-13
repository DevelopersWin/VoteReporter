using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Implementations
{
	sealed class Definition : MappedAspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( new Dictionary<ITypeDefinition, IAspects>
							 {
								 { GeneralizedParameterizedSourceTypeDefinition.Default, ParameterizedSourceAspects.Default },
								 { GeneralizedSpecificationTypeDefinition.Default, SpecificationAspects.Default }
							 }.ToImmutableDictionary() ) {}
	}
}