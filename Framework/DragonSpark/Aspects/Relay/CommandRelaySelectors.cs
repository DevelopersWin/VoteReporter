using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelaySelectors : AspectSelectors<ICommandAdapter, ApplyCommandRelay>
	{
		public static CommandRelaySelectors Default { get; } = new CommandRelaySelectors();
		CommandRelaySelectors() : base( 
			GenericCommandTypeDefinition.Default.ReferencedType, 
			typeof(CommandAdapter<>),
			new MethodAspectDefinition<SpecificationRelay>( CommandTypeDefinition.Default.Validation ),
			new MethodAspectDefinition<CommandRelay>( CommandTypeDefinition.Default.Execution )
		) {}
	}
}