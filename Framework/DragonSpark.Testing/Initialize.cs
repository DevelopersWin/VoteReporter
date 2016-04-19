using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using AssemblyLoader = DragonSpark.Windows.TypeSystem.AssemblyLoader;

namespace DragonSpark.Testing
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Parts() => new LoadPartAssemblyCommand( AssemblyLoader.Instance ).ExecuteWith( typeof(Initialize).Assembly );
	}
}
