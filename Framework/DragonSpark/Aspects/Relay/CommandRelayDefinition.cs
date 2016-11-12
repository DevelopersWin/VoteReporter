using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelayDefinition : PairedAspectBuildDefinition
	{
		public static CommandRelayDefinition Default { get; } = new CommandRelayDefinition();
		CommandRelayDefinition() : base( new Dictionary<ITypeDefinition, IEnumerable<IAspectDefinition>>
										 {
											 { GenericCommandTypeDefinition.Default, CommandRelaySelectors.Default }
										 }.ToImmutableDictionary() ) {}
	}
}