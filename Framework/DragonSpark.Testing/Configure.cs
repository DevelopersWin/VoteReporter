using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Testing
{
	public static class Configure
	{
		[ModuleInitializer( 0 ), DragonSpark.Aspects.Runtime, AssemblyInitialize]
		public static void Initialize()
		{
			LoadPartAssemblyCommand.Instance.Run( typeof(Configure).Assembly );
			// Debugger.Launch();
		}
	}
}
