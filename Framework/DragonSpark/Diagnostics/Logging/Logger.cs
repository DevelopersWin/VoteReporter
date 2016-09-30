using DragonSpark.Configuration;
using DragonSpark.Sources.Parameterized;
using Serilog;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class Logger : LoggerBase
	{
		public static IConfigurableFactory<LoggerConfiguration, ILogger> Configurable { get; } = new Logger();

		public static IParameterizedSource<ILogger> Default { get; } = Configurable.ToCache();
		Logger() {}
	}
}