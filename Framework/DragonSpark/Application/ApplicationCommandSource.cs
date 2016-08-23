using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Setup;

namespace DragonSpark.Application
{
	public class ApplicationCommandSource : CommandSource
	{
		public ApplicationCommandSource( params ICommandSource[] sources ) : base( sources ) {}
		public ApplicationCommandSource( IEnumerable<ICommand> items ) : base( items ) {}
		public ApplicationCommandSource( params ICommand[] items ) : base( items ) {}

		protected override IEnumerable<ICommand> Yield() => base.Yield().Append( new ApplySetup() );
	}
}