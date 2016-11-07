using DragonSpark.Aspects.Relay;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class GeneralizedParameterizedSourceAspect : GeneralizedAspectBase
	{
		readonly static AspectFactory<IRelay, GeneralizedParameterizedSourceAspect> Factory = 
			new AspectFactory<IRelay, GeneralizedParameterizedSourceAspect>( typeof(IParameterizedSource<,>), typeof(ParameterizedSourceRelayAdapter<,>) );

		public GeneralizedParameterizedSourceAspect() : base( Factory.Get ) {}
	}
}