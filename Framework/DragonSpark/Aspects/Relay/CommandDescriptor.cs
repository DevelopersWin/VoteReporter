using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandDescriptor : RelayMethodAspectBuildDefinition<ICommandRelay, ApplyCommandRelay>
	{
		public static CommandDescriptor Default { get; } = new CommandDescriptor();
		CommandDescriptor() : base( 
			CommandTypeDefinition.Default.ReferencedType, GenericCommandTypeDefinition.Default.ReferencedType, 
			typeof(CommandRelayAdapter<>),
			new MethodBasedAspectInstanceLocator<Specification>( CommandTypeDefinition.Default.Validation ),
			new MethodBasedAspectInstanceLocator<CommandRelay>( CommandTypeDefinition.Default.Execution )
		) {}
	}
}