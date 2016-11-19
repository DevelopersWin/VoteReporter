using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Sources;

namespace DragonSpark.Aspects.Diagnostics
{
	sealed class DefaultLoggerConfigurations : ItemSource<ILoggingConfiguration>
	{
		public static DefaultLoggerConfigurations Default { get; } = new DefaultLoggerConfigurations();
		DefaultLoggerConfigurations() : base( new AddSinkConfiguration( LoggingSink.Default ) ) {}
	}
}