namespace DragonSpark.Diagnostics
{
	public class IgnoredOutputCommand : DelegatedTextCommand
	{
		public static IgnoredOutputCommand Default { get; } = new IgnoredOutputCommand();
		IgnoredOutputCommand() : base( s => {} ) {}
	}
}