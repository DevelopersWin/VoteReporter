using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelay : RelayMethodBase
	{
		public CommandRelay() : base( SourceCoercer<ICommandAdapter>.Default.Get ) {}
	}
}