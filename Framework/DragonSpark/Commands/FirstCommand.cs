using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Commands
{
	public class FirstCommand<T> : CommandBase<T>
	{
		readonly IEnumerable<ICommand<T>> commands;

		public FirstCommand( params ICommand<T>[] commands ) : this( commands.AsEnumerable() ) {}

		public FirstCommand( IEnumerable<ICommand<T>> commands )
		{
			this.commands = commands;
		}

		public override void Execute( T parameter )
		{
			foreach ( var command in commands )
			{
				if ( command.CanExecute( parameter ) )
				{
					command.Execute( parameter );
					return;
				}
			}
		}
	}
}