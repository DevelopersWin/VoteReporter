using System;
using DragonSpark.Setup;
using System.Windows.Input;
using DragonSpark.TypeSystem;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication( IServiceProvider provider, params ICommand[] commands ) : base( provider, commands ) {}

		public ConsoleApplication() : this( Default<ICommand>.Items ) {}

		public ConsoleApplication( params ICommand[] commands ) : base( commands ) {}
	}
}