using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Implementations
{
	[LinesOfCodeAvoided( 1 )]
	public sealed class ApplyGeneralizedImplementations : TypeBasedAspectBase
	{
		public ApplyGeneralizedImplementations() : base( Definition.Default ) {}
	}
}
