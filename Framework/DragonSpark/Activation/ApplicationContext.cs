using DragonSpark.Runtime;
using System.Windows.Input;

namespace DragonSpark.Activation
{
	public class ApplicationContext : CompositeCommand
	{
		public ApplicationContext( params ICommand[] commands ) : base( commands ) {}
	}
}