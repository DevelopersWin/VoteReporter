using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplySourceRelayDefinition : ApplyRelayAspectBuildDefinition<IParameterizedSourceRelay, ApplyParameterizedSourceRelay>
	{
		public static ApplySourceRelayDefinition Default { get; } = new ApplySourceRelayDefinition();
		ApplySourceRelayDefinition() : base( 
			GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType, ParameterizedSourceTypeDefinition.Default.ReferencedType, 
			typeof(ParameterizedSourceRelayAdapter<,>),
			new MethodAspectSelector<ParameterizedSourceMethodAspect>( GeneralizedParameterizedSourceTypeDefinition.Default.Method )
		) {}
	}
}