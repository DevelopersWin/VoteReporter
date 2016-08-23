using System.Diagnostics;

namespace DragonSpark.Diagnostics
{
	public class DebugOutputCommand : DelegatedTextCommand
	{
		public static DebugOutputCommand Default { get; } = new DebugOutputCommand();
		DebugOutputCommand() : base( s => Debug.WriteLine( s ) ) {}
	}
}