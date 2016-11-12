using DragonSpark.Commands;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class RunCommandTypeDefinition : ValidatedTypeDefinition
	{
		public static RunCommandTypeDefinition Default { get; } = new RunCommandTypeDefinition();
		RunCommandTypeDefinition() : base( typeof(IRunCommand), nameof(IRunCommand.Execute) ) {}
	}
}