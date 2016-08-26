using DragonSpark.Application;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework.Application
{
	public class Application : Application<AutoData>, IApplication
	{
		public Application() {}
		public Application( params ICommand[] commands ) : base( commands ) {}
	}
}