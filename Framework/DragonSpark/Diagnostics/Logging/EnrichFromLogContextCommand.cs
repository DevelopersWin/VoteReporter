using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Logging
{
	public class EnrichFromLogContextCommand : EnrichCommandBase
	{
		public static EnrichFromLogContextCommand Default { get; } = new EnrichFromLogContextCommand();

		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.FromLogContext();
	}
}