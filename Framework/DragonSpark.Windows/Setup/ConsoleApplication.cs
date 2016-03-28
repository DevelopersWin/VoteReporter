using DragonSpark.Setup;
using System;
using System.Windows.Input;
using DragonSpark.TypeSystem;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication( IServiceProvider provider ) : base( provider ) {}

		public ConsoleApplication() : base( Default<ICommand>.Items ) {}

		public ConsoleApplication( params ICommand[] commands ) : base( commands ) {}
	}
}