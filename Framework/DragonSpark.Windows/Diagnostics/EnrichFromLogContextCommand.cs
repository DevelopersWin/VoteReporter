using DragonSpark.Diagnostics;
using Serilog;
using Serilog.Configuration;

namespace DragonSpark.Windows.Diagnostics
{
	public class EnrichFromLogContextCommand : EnrichCommandBase
	{
		public static EnrichFromLogContextCommand Instance { get; } = new EnrichFromLogContextCommand();

		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.FromLogContext();
	}
}