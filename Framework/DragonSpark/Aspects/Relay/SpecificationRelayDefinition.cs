using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationRelayDefinition : PairedAspectBuildDefinition
	{
		public static SpecificationRelayDefinition Default { get; } = new SpecificationRelayDefinition();
		SpecificationRelayDefinition() : base(
			new Dictionary<ITypeDefinition, IEnumerable<IAspectDefinition>>
			{
				{ SpecificationTypeDefinition.Default, SpecificationSelectors.Default }
			}.ToImmutableDictionary()
		) {}
	}
}