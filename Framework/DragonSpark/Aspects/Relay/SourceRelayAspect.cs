using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(IParameterizedSourceRelay) )]
	public sealed class SourceRelayAspect : ApplyRelayAspectBase, IParameterizedSourceRelay
	{
		readonly IParameterizedSourceRelay relay;

		public SourceRelayAspect() : base( SourceDescriptor.Default ) {}

		public SourceRelayAspect( IParameterizedSourceRelay relay ) : base( SourceDescriptor.Default )
		{
			this.relay = relay;
		}

		public object Get( object parameter ) => relay.Get( parameter );
	}
}