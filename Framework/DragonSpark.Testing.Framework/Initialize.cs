using DragonSpark.Application.Setup;
using DragonSpark.Commands;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework.Runtime;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Command.Default.Execute();

		sealed class Command : DeclarativeSetup
		{
			public static IRunCommand Default { get; } = new Command();
			Command() : base( 
				DragonSpark.Application.Execution.Context.Configured( ExecutionContext.Default )
			) {}
		}
	}
}