using PostSharp.Aspects;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Build
{
	public class IntroducedAspectSelector<TType, TMethod> : AspectSelector<TMethod>
		where TType : ICompositionAspect, ITypeLevelAspect
		where TMethod : IMethodLevelAspect
	{
		public static IntroducedAspectSelector<TType, TMethod> Default { get; } = new IntroducedAspectSelector<TType, TMethod>();
		IntroducedAspectSelector() : base( IntroducedAspectSource<TType>.Default.Yield ) {}

		public IntroducedAspectSelector( ImmutableArray<object> parameters ) : base( new IntroducedAspectSource<TType>( parameters ).Yield ) {}
	}
}