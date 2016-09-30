using DragonSpark.Sources;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Configurations
{
	public class MinimumLevelConfiguration : Scope<LogEventLevel>
	{
		public static MinimumLevelConfiguration Default { get; } = new MinimumLevelConfiguration();
		MinimumLevelConfiguration() : base( () => LogEventLevel.Information ) {}
	}
}
