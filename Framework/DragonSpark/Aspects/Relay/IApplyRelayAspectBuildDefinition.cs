using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Relay
{
	public interface IApplyRelayAspectBuildDefinition : Build.IAspectBuildDefinition, /*ITypeAware,*/ IParameterizedSource<IAspect>, IEnumerable<IAspectSelector> {}
}