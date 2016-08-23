using DragonSpark.Sources;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public class CommandSource : ItemSource<ICommand>, ICommandSource
	{
		protected CommandSource() {}
		public CommandSource( params ICommandSource[] sources ) : this( sources.Concat() ) {}
		public CommandSource( IEnumerable<ICommand> items ) : base( items ) {}
		public CommandSource( params ICommand[] items ) : base( items ) {}
	}
}