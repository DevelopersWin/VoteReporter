using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using System.Collections.Generic;

namespace DragonSpark.Commands
{
	public class CommandCollection : CommandCollection<System.Windows.Input.ICommand>
	{
		public CommandCollection( IEnumerable<System.Windows.Input.ICommand> collection ) : base( collection ) {}
	}

	public class CommandCollection<T> : DeclarativeCollection<T> where T : System.Windows.Input.ICommand
	{
		public CommandCollection() : this( Items<T>.Default ) {}

		public CommandCollection( IEnumerable<T> collection ) : base( collection ) {}
	}
}