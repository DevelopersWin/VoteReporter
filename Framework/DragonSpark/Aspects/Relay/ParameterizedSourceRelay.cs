using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ParameterizedSourceRelay : RelayAspectBase
	{
		public ParameterizedSourceRelay() : base( SourceCoercer<IParameterizedSourceAdapter>.Default.Get ) {}
	}
}