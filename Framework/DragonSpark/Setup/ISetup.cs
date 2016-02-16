using DragonSpark.Runtime;

namespace DragonSpark.Setup
{
	public interface ISetup : ICommand<object> {}

	//public interface ISetup<in T> : ISetup, ICommand<T> {}
}