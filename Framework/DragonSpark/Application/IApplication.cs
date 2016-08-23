using DragonSpark.Commands;
using DragonSpark.Setup;

namespace DragonSpark.Application
{
	public interface IApplication<in T> : ICommand<T>, IApplication {}
}