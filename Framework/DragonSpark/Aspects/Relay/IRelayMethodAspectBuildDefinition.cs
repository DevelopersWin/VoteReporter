using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Relay
{
	public interface IRelayMethodAspectBuildDefinition : Build.IAspectBuildDefinition, ITypeAware, IParameterizedSource<IAspect> {}
}