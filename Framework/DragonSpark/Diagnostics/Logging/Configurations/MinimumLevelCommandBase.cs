using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public abstract class MinimumLevelCommandBase : LoggerConfigurationCommandBase<LoggerMinimumLevelConfiguration>
	{
		protected MinimumLevelCommandBase() : base( configuration => configuration.MinimumLevel ) {}
	}
}