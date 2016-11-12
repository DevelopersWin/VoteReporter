using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SourceRelayDefinition : PairedAspectBuildDefinition
	{
		public static SourceRelayDefinition Default { get; } = new SourceRelayDefinition();
		SourceRelayDefinition() : base( new Dictionary<ITypeDefinition, IEnumerable<IAspectDefinition>>
										{
											{ ParameterizedSourceTypeDefinition.Default, SourceRelaySelectors.Default }
										}.ToImmutableDictionary() ) {}
	}

	public sealed class SourceRelaySelectors : AspectSelectors<IParameterizedSourceAdapter, ApplyParameterizedSourceRelay>
	{
		public static SourceRelaySelectors Default { get; } = new SourceRelaySelectors();
		SourceRelaySelectors() : base( 
			ParameterizedSourceTypeDefinition.Default.ReferencedType, 
			typeof(ParameterizedSourceAdapter<,>),
			new MethodAspectDefinition<ParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default.PrimaryMethod )
		) {}
	}
}