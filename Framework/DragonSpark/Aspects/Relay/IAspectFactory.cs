using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Relay
{
	public interface IAspectFactory : /*IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectSource>>,*/ IParameterizedSource<object, IAspect> {}
}