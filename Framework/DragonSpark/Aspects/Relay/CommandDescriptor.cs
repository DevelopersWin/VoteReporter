using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandDescriptor : SupportedMethodsDefinition<ApplyCommandRelay>
	{
		public static CommandDescriptor Default { get; } = new CommandDescriptor();
		CommandDescriptor() : base( 
			CommandTypeDefinition.Default.DeclaringType, GenericCommandTypeDefinition.Default.DeclaringType, 
			typeof(CommandRelay<>), typeof(ICommandRelay),
			new MethodBasedAspectInstanceLocator<Specification>( CommandTypeDefinition.Default.Validation ),
			new MethodBasedAspectInstanceLocator<Command>( CommandTypeDefinition.Default.Execution )
		) {}
	}
}