using DragonSpark.Aspects.Adapters;
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
			new Dictionary<ITypeDefinition, IEnumerable<IAspectSelector>>
			{
				{ GenericSpecificationTypeDefinition.Default, SpecificationSelectors.Default }
			}.ToImmutableDictionary()
		) {}
	}

	public sealed class SpecificationSelectors : AspectSelectors<ISpecificationRelayAdapter, ApplySpecificationRelay>
	{
		public static SpecificationSelectors Default { get; } = new SpecificationSelectors();
		SpecificationSelectors() : base( 
			GenericSpecificationTypeDefinition.Default.ReferencedType, 
			typeof(SpecificationAdapter<>),
			new MethodAspectSelector<SpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default.PrimaryMethod )
		) {}
	}
}