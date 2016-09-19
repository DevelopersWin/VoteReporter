using System.Windows.Input;

namespace DragonSpark.Aspects
{
	public sealed class CommandDefinition : ValidatedComponentDefinition
	{
		public static CommandDefinition Default { get; } = new CommandDefinition();
		CommandDefinition() : base( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) {}
	}
}