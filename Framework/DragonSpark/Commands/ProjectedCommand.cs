using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using DragonSpark.TypeSystem;

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

		public override void Execute( [Optional]T parameter ) => command.Execute( projection( parameter ) );
	}
}