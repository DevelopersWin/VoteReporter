using DragonSpark.Activation;
using PostSharp.Aspects;
using System.Diagnostics;
using ExecutionContext = DragonSpark.Testing.Framework.Setup.ExecutionContext;

namespace DragonSpark.Testing.Framework
{
	public static class Configure
	{
		[ModuleInitializer( 0 ), Aspects.Runtime]
		public static void Execute()
		{
			// InitializeJetBrainsTaskRunnerCommand.Instance.Run( AppDomain.CurrentDomain.SetupInformation );
			Execution.Initialize( ExecutionContext.Instance );
			Trace.WriteLine( $"Initializing {typeof(Configure)}" );
		}
	}
}