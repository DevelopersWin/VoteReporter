using DragonSpark.Diagnostics.Logging.Configurations;
using DragonSpark.Sources;
using Serilog.Core;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class LoggingController : Scope<LoggingLevelSwitch>
	{
		public static LoggingController Default { get; } = new LoggingController();
		LoggingController() : base( Factory.GlobalCache( () => new LoggingLevelSwitch( MinimumLevelConfiguration.Default.Get() ) ) ) {}
	}
}