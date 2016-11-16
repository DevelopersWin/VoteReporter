using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class ReadFromConfigurationBase : LoggingConfigurationBase<LoggerSettingsConfiguration>
	{
		protected ReadFromConfigurationBase() : base( configuration => configuration.ReadFrom ) {}
	}
}