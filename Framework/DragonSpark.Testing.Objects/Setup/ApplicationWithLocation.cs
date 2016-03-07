using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.TypeSystem;

namespace DragonSpark.Testing.Objects.Setup
{
	public class ApplicationWithLocation<T> : Application<T> where T : ICommand
	{
		public ApplicationWithLocation() : this( Default<ICommand>.Items  ) {}

		public ApplicationWithLocation( IEnumerable<ICommand> commands ) : base( commands.Append( new AssignLocationCommand() ) ) {}
	}
	
}