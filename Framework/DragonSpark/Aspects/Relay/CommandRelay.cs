using DragonSpark.Aspects.Adapters;
using JetBrains.Annotations;

namespace DragonSpark.Aspects.Relay
{
	[UsedImplicitly]
	public sealed class CommandRelay : MethodAspectBase
	{
		public CommandRelay() : base( o => o is ICommandRelay ) {}
	}
}