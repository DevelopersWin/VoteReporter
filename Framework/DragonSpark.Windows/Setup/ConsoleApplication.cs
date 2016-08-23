using DragonSpark.Application;
using System.Windows.Input;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication() {}
		public ConsoleApplication( params ICommand[] commands ) : base( commands ) {}
	}
}