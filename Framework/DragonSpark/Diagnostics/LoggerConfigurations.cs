using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerConfigurations : ItemScope<ILoggingConfiguration>
	{
		public static LoggerConfigurations Default { get; } = new LoggerConfigurations();
		LoggerConfigurations() : base( DefaultLoggerConfigurations.Default.IncludeExports ) {}
	}
}