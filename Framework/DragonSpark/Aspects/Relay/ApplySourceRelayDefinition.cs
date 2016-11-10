using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplySourceRelayDefinition : AspectBuildDefinition<IParameterizedSourceAdapter, ApplyParameterizedSourceRelay>
	{
		public static ApplySourceRelayDefinition Default { get; } = new ApplySourceRelayDefinition();
		ApplySourceRelayDefinition() : base( 
			ParameterizedSourceTypeDefinition.Default.ReferencedType, 
			typeof(ParameterizedSourceAdapter<,>),
			new MethodAspectSource<ParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default.Method )
		) {}
	}
}