using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Windows.Testing
{
	public static class Configure
	{
		/*[ModuleInitializer( 0 ), Aspects.Runtime, AssemblyInitialize]
		public static void Initialize()
		{
			lock ( Properties.Settings.Default )
			{
				Properties.Settings.Default.Reset();
			}
			new LoadPartAssemblyCommand( new TypeSystem.AssemblyLoader( assembly => "DragonSpark.Testing" ) ).Run( typeof(Configure).Assembly );
		}*/
	}
}