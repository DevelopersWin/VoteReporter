using System.Windows.Input;

namespace DragonSpark.Commands
{
	public interface IRunCommand : ICommand
	{
		void Execute();
	}
}