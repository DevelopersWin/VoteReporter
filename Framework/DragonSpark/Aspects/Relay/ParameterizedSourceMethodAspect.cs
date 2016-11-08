namespace DragonSpark.Aspects.Relay
{
	public sealed class ParameterizedSourceMethodAspect : MethodAspectBase
	{
		public ParameterizedSourceMethodAspect() : base( o => o is IParameterizedSourceRelay ) {}
	}
}