using System.Collections.Generic;

namespace DragonSpark.Runtime
{
	public class CommandCollection : CommandCollection<System.Windows.Input.ICommand>
	{
		public CommandCollection()
		{}

		public CommandCollection( IEnumerable<System.Windows.Input.ICommand> collection ) : base( collection )
		{}
	}

	public class CommandCollection<T> : DeclarativeCollection<T> where T : System.Windows.Input.ICommand
	{
		public CommandCollection()
		{}

		public CommandCollection( IEnumerable<T> collection ) : base( collection )
		{}
	}
}