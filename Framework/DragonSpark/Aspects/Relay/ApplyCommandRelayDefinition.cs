using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplyCommandRelayDefinition : AspectBuildDefinition<ICommandRelay, ApplyCommandRelay>
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