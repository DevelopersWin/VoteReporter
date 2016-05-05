using DragonSpark.Extensions;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;

namespace DragonSpark.Testing
{
	public static class Initialize
	{
		[ModuleInitializer( 0 ), DragonSpark.Aspects.Runtime]
		public static void Execute()
		{
			LoadPartAssemblyCommand.Instance.Run( typeof(Initialize).Assembly );
		}
	}
}
