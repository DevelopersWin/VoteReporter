using DragonSpark.Setup;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication() {}
		public ConsoleApplication( SystemParts parts ) : base( parts ) {}
		public ConsoleApplication( SystemParts parts, IServiceProvider services ) : base( parts, services ) {}
		public ConsoleApplication( SystemParts parts, IServiceProvider services, IEnumerable<ICommand> commands ) : base( parts, services, commands ) {}
	}
}