using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SourceRelayDefinition : AspectBuildDefinition
	{
		public static SourceRelayDefinition Default { get; } = new SourceRelayDefinition();
		SourceRelayDefinition() : base(
			new AspectDefinitionSelector( definition => definition.Select( method => new MethodAspects<ParameterizedSourceRelay>( method ) ) ),
			GeneralizedParameterizedSourceTypeDefinition.Default
		) {}
	}
}