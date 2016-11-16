using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using LoggerConfiguration = Serilog.LoggerConfiguration;

namespace DragonSpark.Diagnostics
{
	public sealed class DefaultSystemLoggerConfigurations : ItemSource<IAlteration<LoggerConfiguration>>
	{
		public static DefaultSystemLoggerConfigurations Default { get; } = new DefaultSystemLoggerConfigurations();
		DefaultSystemLoggerConfigurations() : base( AddHistorySink.Implementation.Append( DefaultLoggerConfigurations.Default ) ) {}

		sealed class AddHistorySink : AddSinkConfiguration
		{
			public static AddHistorySink Implementation { get; } = new AddHistorySink();
			AddHistorySink() : base( LoggingHistory.Default ) {}
		}
	}
}