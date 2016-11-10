using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplyCommandRelayDefinition : AspectBuildDefinition<ICommandAdapter, ApplyCommandRelay>
	{
		public static ApplyCommandRelayDefinition Default { get; } = new ApplyCommandRelayDefinition();
		ApplyCommandRelayDefinition() : base( 
			GenericCommandTypeDefinition.Default.ReferencedType, 
			typeof(CommandAdapter<>),
			new MethodAspectSource<SpecificationRelay>( CommandTypeDefinition.Default.Validation ),
			new MethodAspectSource<CommandRelay>( CommandTypeDefinition.Default.Execution )
		) {}
	}
}