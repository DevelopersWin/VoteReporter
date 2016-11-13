using DragonSpark.Commands;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class CommandAdapter<T> : CommandAdapterBase<T>, ICommandAdapter
	{
		readonly ICommand<T> command;

		public CommandAdapter( ICommand<T> command ) : base( SourceCoercer<ICoercerAdapter>.Default.Get( command )?.To( DefaultCoercer ) ?? DefaultCoercer )
		{
			this.command = command;
		}

		protected override void Execute( T parameter ) => command.Execute( parameter );
	}
}