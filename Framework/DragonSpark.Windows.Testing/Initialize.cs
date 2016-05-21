using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Windows.Testing
{
	public static class Initialize
	{
		[ModuleInitializer( 0 ), Aspects.Runtime]
		public static void Execute()
		{
			new LoadPartAssemblyCommand( new TypeSystem.AssemblyLoader( assembly => "DragonSpark.Testing" ) ).Run( typeof(Initialize).Assembly );
			lock ( Properties.Settings.Default )
			{
				Properties.Settings.Default.Reset();
			}
		}
	}
}