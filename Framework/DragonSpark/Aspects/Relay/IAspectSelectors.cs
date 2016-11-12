using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Relay
{
	public interface IAspectSelectors : IItemSource<IAspectDefinition>, IParameterizedSource<object, IAspect> {}
}