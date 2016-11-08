using DragonSpark.Commands;
using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelayAdapter<T> : DelegatedInvocation<T, bool>, ICommandRelay
	{
		readonly ICommand<T> command;

		public CommandRelayAdapter( ICommand<T> command ) : base( command.IsSatisfiedBy )
		{
			this.command = command;
		}

		public void Execute( object parameter ) => command.Execute( parameter.AsValid<T>() );
	}
}