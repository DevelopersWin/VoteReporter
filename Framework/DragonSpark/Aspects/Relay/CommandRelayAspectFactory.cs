using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelayAspectFactory : AspectFactory<ICommandAdapter, ApplyCommandRelay>
	{
		public static CommandRelayAspectFactory Default { get; } = new CommandRelayAspectFactory();
		CommandRelayAspectFactory() : base( GenericCommandTypeDefinition.Default.ReferencedType, typeof(CommandAdapter<>) ) {}
	}
}