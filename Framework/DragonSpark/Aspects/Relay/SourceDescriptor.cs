using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SourceDescriptor : RelayMethodDefinition<IParameterizedSourceRelay, ApplyParameterizedSourceRelay>
	{
		public static SourceDescriptor Default { get; } = new SourceDescriptor();
		SourceDescriptor() : base( 
			GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType, ParameterizedSourceTypeDefinition.Default.ReferencedType, 
			typeof(ParameterizedSourceRelayAdapter<,>),
			new MethodBasedAspectInstanceLocator<ParameterizedSourceMethodAspect>( GeneralizedParameterizedSourceTypeDefinition.Default.Method )
		) {}
	}
}