using System.Threading;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Testing
{
	public static class Configure
	{
		[ModuleInitializer( 0 ), /*DragonSpark.Aspects.Runtime, AssemblyInitialize*/]
		public static void Initialize()
		{
			Thread.Sleep( 2000 );
			AssemblyInitializer.Instance.Run( typeof(Parts.Development.Configure).Assembly );
			// LoadPartAssemblyCommand.Instance.Run( typeof(Configure).Assembly );
			// Thread.Sleep(2000 );
		}
	}
}
