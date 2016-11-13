using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using System.Collections.Generic;

namespace DragonSpark.Commands
{
	public class CommandCollection : DeclarativeCollection<System.Windows.Input.ICommand>
	{
		public CommandCollection( IEnumerable<System.Windows.Input.ICommand> collection ) : base( collection ) {}
	}

	public class CommandCollection<T> : DeclarativeCollection<ICommand<T>>
	{
		public CommandCollection() : this( Items<ICommand<T>>.Default ) {}

		public CommandCollection( IEnumerable<ICommand<T>> collection ) : base( collection ) {}
	}
}