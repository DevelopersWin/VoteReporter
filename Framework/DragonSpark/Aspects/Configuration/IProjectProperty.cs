using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Configuration
{
	public interface IProjectProperty : IParameterizedSource<IProject, string>, ISource<string> {}
}