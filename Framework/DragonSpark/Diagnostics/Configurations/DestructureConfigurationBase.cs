using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class DestructureConfigurationBase : LoggingConfigurationBase<LoggerDestructuringConfiguration>
	{
		protected DestructureConfigurationBase() : base( configuration => configuration.Destructure ) {}
	}
}