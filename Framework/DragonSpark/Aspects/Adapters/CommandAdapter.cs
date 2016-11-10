using DragonSpark.Commands;
using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class CommandAdapter<T> : SpecificationAdapter<T>, ICommandAdapter
	{
		readonly ICommand<T> command;

		public CommandAdapter( ICommand<T> command ) : base( command.IsSatisfiedBy )
		{
			this.command = command;
		}

		public void Execute( object parameter ) => command.Execute( parameter.AsValid<T>() );
	}
}