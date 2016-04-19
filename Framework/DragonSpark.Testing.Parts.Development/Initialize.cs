using PostSharp.Aspects;
using Serilog.Events;

namespace DragonSpark.Testing.Parts.Development
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Level() => Diagnostics.Configuration.Initialize( LogEventLevel.Debug );
	}
}
