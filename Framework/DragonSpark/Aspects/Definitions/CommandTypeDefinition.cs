using System.Windows.Input;
using DragonSpark.Commands;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class CommandTypeDefinition : ValidatedTypeDefinition
	{
		public static CommandTypeDefinition Default { get; } = new CommandTypeDefinition();
		CommandTypeDefinition() : base( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) {}
	}

	public sealed class RunCommandTypeDefinition : ValidatedTypeDefinition
	{
		public static RunCommandTypeDefinition Default { get; } = new RunCommandTypeDefinition();
		RunCommandTypeDefinition() : base( typeof(IRunCommand), nameof(IRunCommand.Execute) ) {}
	}
}