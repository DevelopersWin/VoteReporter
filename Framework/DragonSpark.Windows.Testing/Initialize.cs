using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Windows.Testing
{
	public static class Initialize
	{
		[ModuleInitializer( 0 ), DragonSpark.Testing.Framework.Runtime]
		public static void Execute()
		{
			Properties.Settings.Default.Reset();
			new LoadPartAssemblyCommand( new TypeSystem.AssemblyLoader( assembly => "DragonSpark.Testing" ) ).Run( typeof(Initialize).Assembly );
		}
	}
}