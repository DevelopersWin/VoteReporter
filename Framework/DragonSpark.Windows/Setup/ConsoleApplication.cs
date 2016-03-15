using DragonSpark.Setup;
using System.Windows.Input;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication( IApplicationContext context, params ICommand[] commands ) : base( context, commands ) {}

		public ConsoleApplication( params ICommand[] commands ) : base( commands ) {}
	}
}