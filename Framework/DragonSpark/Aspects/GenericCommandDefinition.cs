using System.Windows.Input;
using DragonSpark.Commands;

namespace DragonSpark.Aspects
{
	public sealed class GenericCommandDefinition : ValidatedComponentDefinition
	{
		public static GenericCommandDefinition Default { get; } = new GenericCommandDefinition();
		GenericCommandDefinition() : base( typeof(ICommand<>), nameof(ICommand.Execute) ) {}
	}
}