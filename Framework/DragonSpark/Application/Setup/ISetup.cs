using DragonSpark.Commands;

namespace DragonSpark.Application.Setup
{
	public interface ISetup : ICommand<object>, IPriorityAware {}
}