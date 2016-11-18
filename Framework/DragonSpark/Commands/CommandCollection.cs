using DragonSpark.Runtime;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public class CommandCollection : DeclarativeCollection<ICommand>
	{
		public CommandCollection( IEnumerable<ICommand> collection ) : base( collection ) {}
	}
}