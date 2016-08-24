using DragonSpark.Application.Setup;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework.Runtime;
using PostSharp.Aspects;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Command.Default.Run();

		sealed class Command : Setup
		{
			public static ICommand Default { get; } = new Command();
			Command() : base( 
				DragonSpark.Application.Execution.Context.Configured( ExecutionContext.Default )
			) {}
		}
	}
}