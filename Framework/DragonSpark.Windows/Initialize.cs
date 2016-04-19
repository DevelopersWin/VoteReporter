using DragonSpark.Diagnostics.Logger;
using DragonSpark.Windows.Diagnostics;
using PostSharp.Aspects;

namespace DragonSpark.Windows
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Profiler() => DragonSpark.Diagnostics.Configuration.Initialize( typeof(ProfilerFactory<Category.Debug>) );
	}
}
