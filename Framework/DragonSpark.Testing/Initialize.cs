using DragonSpark.Extensions;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Testing
{
	// [Synchronized]
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Parts() => LoadPartAssemblyCommand.Instance.Run( typeof(Initialize).Assembly );
	}
}
