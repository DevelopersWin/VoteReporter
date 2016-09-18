using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Windows
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execute()
		{
			Application.Execution.Context.Assign( ExecutionContext.Default );
			DragonSpark.TypeSystem.Configuration.AssemblyLoader.Assign( o => Assembly.LoadFile );
			DragonSpark.TypeSystem.Configuration.AssemblyPathLocator.Assign( o => AssemblyLocator.Default.Get );
		}
	}
}