using DragonSpark.Sources;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public class FixedCommandSource : ItemSource<ICommand>, ICommandSource
	{
		protected FixedCommandSource() {}
		public FixedCommandSource( params ICommandSource[] sources ) : this( sources.Concat() ) {}
		public FixedCommandSource( IEnumerable<ICommand> items ) : base( items ) {}
		public FixedCommandSource( params ICommand[] items ) : base( items ) {}
	}
}