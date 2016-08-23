using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public abstract class ReadFromCommandBase : LoggerConfigurationCommandBase<LoggerSettingsConfiguration>
	{
		protected ReadFromCommandBase() : base( configuration => configuration.ReadFrom ) {}
	}
}