using DragonSpark.Sources.Coercion;
using JetBrains.Annotations;

namespace DragonSpark.Aspects.Relay
{
	[UsedImplicitly]
	public sealed class CommandRelay : MethodAspectBase
	{
		public CommandRelay() : base( CastCoercer<ICommandRelay>.Default.Get ) {}
	}
}