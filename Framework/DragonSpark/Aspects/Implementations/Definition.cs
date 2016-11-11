using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Implementations
{
	sealed class Definition : PairedAspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( new Dictionary<ITypeDefinition, IAspectSelector>
							 {
								 { GeneralizedParameterizedSourceTypeDefinition.Default, ParameterizedSourceAspectSelector.Default },
								 { GeneralizedSpecificationTypeDefinition.Default, SpecificationAspectSelector.Default }
							 }.ToImmutableDictionary() ) {}
	}
}