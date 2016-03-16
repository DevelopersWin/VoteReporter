using System;
using DragonSpark.Setup;
using System.Windows.Input;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication( IServiceProvider provider, params ICommand[] commands ) : base( provider, commands ) {}

		public ConsoleApplication( params ICommand[] commands ) : base( commands ) {}
	}
}