using Serilog.Configuration;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Configurations
{
	public class MinimumLevelIsConfiguration : MinimumLevelConfigurationBase
	{
		public MinimumLevelIsConfiguration() : this( LogEventLevel.Information ) {}

		public MinimumLevelIsConfiguration( LogEventLevel level )
		{
			Level = level;
		}

		public LogEventLevel Level { get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.Is( Level );
	}
}