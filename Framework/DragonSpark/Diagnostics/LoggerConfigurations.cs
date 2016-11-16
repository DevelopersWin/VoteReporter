using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerConfigurations : ItemScope<IAlteration<LoggerConfiguration>>
	{
		public static LoggerConfigurations Default { get; } = new LoggerConfigurations();
		LoggerConfigurations() : base( DefaultLoggerConfigurations.Default.IncludeExports ) {}
	}
}