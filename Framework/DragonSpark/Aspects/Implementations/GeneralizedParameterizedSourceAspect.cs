using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class GeneralizedParameterizedSourceAspect : GeneralizedAspectBase
	{
		/*readonly IParameterizedSourceRelay relay;

		readonly static AspectFactory<IParameterizedSourceRelay, GeneralizedParameterizedSourceAspect> Factory = 
			new AspectFactory<IParameterizedSourceRelay, GeneralizedParameterizedSourceAspect>( typeof(IParameterizedSource<,>), typeof(ParameterizedSourceRelayAdapter<,>) );

		public GeneralizedParameterizedSourceAspect() : base( Factory.Get ) {}*/

		public GeneralizedParameterizedSourceAspect( IParameterizedSourceRelay relay )
		{
			// this.relay = relay;
		}
	}
}