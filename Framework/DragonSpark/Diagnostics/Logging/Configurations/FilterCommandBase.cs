using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public abstract class FilterCommandBase : LoggerConfigurationCommandBase<LoggerFilterConfiguration>
	{
		protected FilterCommandBase() : base( configuration => configuration.Filter ) {}
	}
}