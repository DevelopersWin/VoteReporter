using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Relay
{
	public interface IRelayMethodDefinition : Build.IDefinition, ITypeAware, IParameterizedSource<IAspect> {}
}