using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class EnrichConfigurationBase : LoggingConfigurationBase<LoggerEnrichmentConfiguration>
	{
		protected EnrichConfigurationBase() : base( configuration => configuration.Enrich ) {}
	}
}