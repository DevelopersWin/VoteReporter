using DragonSpark.Commands;
using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelayAdapter<T> : SpecificationRelayAdapter<T>, ICommandRelay
	{
		readonly ICommand<T> command;

		public CommandRelayAdapter( ICommand<T> command ) : base( command )
		{
			this.command = command;
		}

		public void Execute( object parameter ) => command.Execute( parameter.AsValid<T>() );
	}
}