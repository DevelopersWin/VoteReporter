using DragonSpark.Sources.Coercion;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ParameterizedSourceMethodAspect : MethodAspectBase
	{
		public ParameterizedSourceMethodAspect() : base( CastCoercer<IParameterizedSourceRelay>.Default.Get ) {}
	}
}