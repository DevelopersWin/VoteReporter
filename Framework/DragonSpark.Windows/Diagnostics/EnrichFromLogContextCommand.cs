using DragonSpark.Diagnostics;
using Serilog;
using Serilog.Configuration;

namespace DragonSpark.Windows.Diagnostics
{
	public class EnrichFromLogContextCommand : EnrichCommandBase
	{
		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.FromLogContext();
	}
}