using DragonSpark.Diagnostics;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Testing.Framework.Runtime;
using PostSharp.Aspects;
using System.Linq;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution()
		{
			DragonSpark.Application.Execution.Context.Assign( ExecutionContext.Default );
			Logger.Configurable.Configurators.Assign( o => new LoggerExportedConfigurations( DefaultSystemLoggerConfigurations.Default.Get().ToArray() ).Get().Wrap() );
		}

		/*sealed class Command : CompositeCommand
		{
			public static IRunCommand Default { get; } = new Command();
			Command() : base( 
				DragonSpark.Application.Execution.Context.Configured( ExecutionContext.Default )
			) {}
		}*/
	}

	
}