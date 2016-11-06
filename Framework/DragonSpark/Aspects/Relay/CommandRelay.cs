using DragonSpark.Commands;
using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelay<T> : SpecificationRelay<T>, ICommandRelay
	{
		readonly ICommand<T> command;

		public CommandRelay( ICommand<T> command ) : base( command )
		{
			this.command = command;
		}

		public void Execute( object parameter ) => command.Execute( parameter.AsValid<T>() );
	}
}