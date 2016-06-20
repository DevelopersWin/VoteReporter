using DragonSpark.Activation;
using DragonSpark.Aspects;
using PostSharp.Aspects;
using System.Diagnostics;
using ExecutionContext = DragonSpark.Testing.Framework.Setup.ExecutionContextStore;

namespace DragonSpark.Testing.Framework
{
	public static class Configure
	{
		[ModuleInitializer( 0 ), Aspects.Runtime, AssemblyInitialize]
		public static void Initialize()
		{
			Execution.Initialize( ExecutionContext.Instance );
			Trace.WriteLine( $"Initializing {typeof(Configure)}" );
		}
	}
}