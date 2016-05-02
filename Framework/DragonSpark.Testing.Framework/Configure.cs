using System;
using System.Diagnostics;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Aspects;
using ExecutionContext = DragonSpark.Testing.Framework.Setup.ExecutionContext;

namespace DragonSpark.Testing.Framework
{
	public static class Configure
	{
		[ModuleInitializer( 0 ), Runtime]
		public static void Execute()
		{
			InitializeJetBrainsTaskRunnerCommand.Instance.Run( AppDomain.CurrentDomain.SetupInformation );
			Execution.Initialize( ExecutionContext.Instance );
			Trace.WriteLine( $"Initializing {typeof(Configure)}" );
		}
	}
}