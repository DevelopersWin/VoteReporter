using DragonSpark.TypeSystem;
using System;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public class ProjectedCommand<T> : CommandBase<T>
	{
		readonly ICommand command;
		readonly Func<T, object> projection;

		public ProjectedCommand( ICommand command ) : this( command, Delegates<T>.Object ) {}

		public ProjectedCommand( ICommand command, Func<T, object> projection )
		{
			this.command = command;
			this.projection = projection;
		}

		public override void Execute( T parameter ) => command.Execute( projection( parameter ) );
	}
}