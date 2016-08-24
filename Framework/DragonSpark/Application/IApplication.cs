using DragonSpark.Application.Setup;
using DragonSpark.Commands;

namespace DragonSpark.Application
{
	public interface IApplication<in T> : ICommand<T>, IApplication {}
}