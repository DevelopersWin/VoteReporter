using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class FilterConfigurationBase : LoggingConfigurationBase<LoggerFilterConfiguration>
	{
		protected FilterConfigurationBase() : base( configuration => configuration.Filter ) {}
	}
}