using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Windows.Testing
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Parts()
		{
			var assemblyLoader = new TypeSystem.AssemblyLoader( assembly => "DragonSpark.Testing" );
			var command = new LoadPartAssemblyCommand( assemblyLoader );
			command.Run( typeof(Initialize).Assembly );
		}
	}
}
