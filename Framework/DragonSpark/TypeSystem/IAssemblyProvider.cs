using DragonSpark.Activation;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public interface IAssemblyProvider : IFactory<ImmutableArray<Assembly>> {}
}