using DragonSpark.Activation.IoC;
using DragonSpark.Runtime;
using DragonSpark.Testing.Framework.Setup;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Testing.Objects.Setup
{
	public class ApplicationWithLocation<T> : Application<T> where T : ICommand
	{
		public ApplicationWithLocation( params ICommand<AutoData>[] commands ) : base( commands.Concat( new [] { new AssignLocationCommand() } ).ToArray() ) {}
	}
	
}