using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplyCommandRelayDefinition : ApplyRelayAspectBuildDefinition<ICommandRelay, ApplyCommandRelay>
	{
		public static ApplyCommandRelayDefinition Default { get; } = new ApplyCommandRelayDefinition();
		ApplyCommandRelayDefinition() : base( 
			CommandTypeDefinition.Default.ReferencedType, GenericCommandTypeDefinition.Default.ReferencedType, 
			typeof(CommandRelayAdapter<>),
			new MethodAspectSelector<Specification>( CommandTypeDefinition.Default.Validation ),
			new MethodAspectSelector<CommandRelay>( CommandTypeDefinition.Default.Execution )
		) {}
	}
}