using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System.Windows.Input;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication() : base( Items<ICommand>.Default ) {}

		public ConsoleApplication( params ICommand[] commands ) : base( commands ) {}
	}
}