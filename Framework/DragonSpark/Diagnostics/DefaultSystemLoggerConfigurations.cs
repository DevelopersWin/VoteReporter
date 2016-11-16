using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources;

namespace DragonSpark.Diagnostics
{
	public sealed class DefaultSystemLoggerConfigurations : ItemSource<ILoggingConfiguration>
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