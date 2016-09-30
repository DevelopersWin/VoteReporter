using DragonSpark.Configuration;
using DragonSpark.Sources.Parameterized;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public sealed class SystemLogger : LoggerBase
	{
		public static IConfigurableFactory<LoggerConfiguration, ILogger> Configurable { get; } = new SystemLogger();

		public static IParameterizedSource<ILogger> Default { get; } = Configurable.ToCache();
		SystemLogger() : base( DefaultSystemLoggerConfigurations.Default.Get() ) {}
	}
}