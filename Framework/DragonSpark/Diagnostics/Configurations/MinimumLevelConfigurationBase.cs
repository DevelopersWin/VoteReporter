using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class MinimumLevelConfigurationBase : LoggingConfigurationBase<LoggerMinimumLevelConfiguration>
	{
		protected MinimumLevelConfigurationBase() : base( configuration => configuration.MinimumLevel ) {}
	}
}