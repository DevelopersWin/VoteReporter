using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public class EnrichFromLogContextConfiguration : EnrichConfigurationBase
	{
		public static EnrichFromLogContextConfiguration Default { get; } = new EnrichFromLogContextConfiguration();

		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.FromLogContext();
	}
}