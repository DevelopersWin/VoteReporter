using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SourceRelaySelectors : AspectSelectors<IParameterizedSourceAdapter, ApplyParameterizedSourceRelay>
	{
		public static SourceRelaySelectors Default { get; } = new SourceRelaySelectors();
		SourceRelaySelectors() : base( 
			ParameterizedSourceTypeDefinition.Default.ReferencedType, 
			typeof(ParameterizedSourceAdapter<,>),
			new MethodAspectDefinition<ParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default.PrimaryMethod )
		) {}
	}
}