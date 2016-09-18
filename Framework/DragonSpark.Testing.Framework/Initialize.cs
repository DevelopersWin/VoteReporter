using DragonSpark.Testing.Framework.Runtime;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => DragonSpark.Application.Execution.Context.Assign( ExecutionContext.Default );
		
		/*sealed class Command : CompositeCommand
		{
			public static IRunCommand Default { get; } = new Command();
			Command() : base( 
				DragonSpark.Application.Execution.Context.Configured( ExecutionContext.Default )
			) {}
		}*/
	}
}