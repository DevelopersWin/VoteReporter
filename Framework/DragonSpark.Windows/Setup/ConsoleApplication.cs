using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication( params ICommand[] commands ) : this( Default<Assembly>.Items, commands ) {}

		public ConsoleApplication( Assembly[] assemblies, params ICommand[] commands ) : base( assemblies, new ApplicationCommandFactory<string[]>( commands ) ) {}
	}
}