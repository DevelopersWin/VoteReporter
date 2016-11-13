using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Relay
{
	[LinesOfCodeAvoided( 1 )]
	public sealed class ApplyRelays : TypeBasedAspectBase
	{
		public ApplyRelays() : base( Definitions.Default ) {}
	}
}