using System.Linq;
using DragonSpark.Configuration;
using DragonSpark.Sources.Parameterized;
using Serilog;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class LoggerExportedConfigurations : ConfigurationSource<LoggerConfiguration>
	{
		public static LoggerExportedConfigurations Default { get; } = new LoggerExportedConfigurations();
		LoggerExportedConfigurations() : this( DefaultLoggerAlterations.Default.Get().ToArray() ) {}
		public LoggerExportedConfigurations( params IAlteration<LoggerConfiguration>[] configurators ) : base( configurators ) {}
	}
}