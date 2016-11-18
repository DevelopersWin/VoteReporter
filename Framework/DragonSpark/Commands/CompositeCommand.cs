using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Defaults = DragonSpark.Sources.Parameterized.Defaults;

namespace DragonSpark.Commands
{
	public class CompositeCommand : CompositeCommand<object>, IRunCommand
	{
		public CompositeCommand( params ICommand[] commands ) : this( commands.AsEnumerable() ) {}

		public CompositeCommand( IEnumerable<ICommand> commands ) : base( commands.Select( command => command.Adapt<object>() ) ) {}

		public void Execute() => Execute( Defaults.Parameter );
	}

	public class CompositeCommand<T> : CommandBase<T>
	{
		readonly IEnumerable<ICommand<T>> commands;

		public CompositeCommand( params ICommand<T>[] commands ) : this( commands.AsEnumerable() ) {}

		public CompositeCommand( IEnumerable<ICommand<T>> commands )
		{
			this.commands = commands;
		}

		public override void Execute( T parameter )
		{
			foreach ( var command in commands )
			{
				command.Execute( parameter );
			}
		}
	}
}