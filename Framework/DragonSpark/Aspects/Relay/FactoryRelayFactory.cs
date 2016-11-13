using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class FactoryRelayFactory : AspectFactory<IParameterizedSourceAdapter, ApplyParameterizedSourceRelay>
	{
		public static FactoryRelayFactory Default { get; } = new FactoryRelayFactory();
		FactoryRelayFactory() : base( ParameterizedSourceTypeDefinition.Default.ReferencedType, typeof(ParameterizedSourceAdapter<,>) ) {}
	}
}