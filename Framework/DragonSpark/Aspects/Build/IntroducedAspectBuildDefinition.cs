using System.Collections.Immutable;
using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public class IntroducedAspectBuildDefinition<TType, TMethod> : AspectBuildDefinition
		where TType : ICompositionAspect, ITypeLevelAspect
		where TMethod : IMethodLevelAspect
	{
		public IntroducedAspectBuildDefinition( ImmutableArray<object> parameters, params ITypeDefinition[] candidates ) 
			: base( new IntroducedAspectSelector<TType, TMethod>( parameters ), candidates ) {}
	}
}