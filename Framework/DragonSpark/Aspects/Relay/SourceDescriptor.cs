using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SourceDescriptor : SupportedMethodsDefinition<SourceRelayAspect>
	{
		public static SourceDescriptor Default { get; } = new SourceDescriptor();
		SourceDescriptor() : base( 
			GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType, ParameterizedSourceTypeDefinition.Default.ReferencedType, 
			typeof(ParameterizedSourceRelay<,>), typeof(IParameterizedSourceRelay),
			new MethodBasedAspectInstanceLocator<ParameterizedSourceMethodAspect>( GeneralizedParameterizedSourceTypeDefinition.Default.Method )
		) {}
	}
}