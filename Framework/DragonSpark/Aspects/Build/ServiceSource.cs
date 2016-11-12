using DragonSpark.Sources;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Build
{
	public class ServiceSource<T> : DelegatedSource<T> where T : class, IService
	{
		public static ServiceSource<T> Default { get; } = new ServiceSource<T>();
		ServiceSource() : base( () => PostSharpEnvironment.CurrentProject.GetService<T>() ) {}
	}
}