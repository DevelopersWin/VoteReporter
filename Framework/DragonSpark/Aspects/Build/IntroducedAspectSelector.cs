using DragonSpark.Extensions;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public class IntroducedAspectSelector<TType, TMethod> : AspectSelector<TType, TMethod>
		where TType : CompositionAspect
		where TMethod : IAspect
	{
		public new static IntroducedAspectSelector<TType, TMethod> Default { get; } = new IntroducedAspectSelector<TType, TMethod>();
		IntroducedAspectSelector() : base( definition => new IntroducedTypeAspectDefinition<TType>( definition ).Yield() ) {}
	}
}