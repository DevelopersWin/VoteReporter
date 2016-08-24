using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.Application.Setup;
using DragonSpark.Commands;
using DragonSpark.Extensions;

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