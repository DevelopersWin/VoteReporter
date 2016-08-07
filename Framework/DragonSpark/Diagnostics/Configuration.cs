using DragonSpark.Runtime;
using Serilog.Events;

namespace DragonSpark.Diagnostics
{
	public class MinimumLevelConfiguration : CachedParameterizedScope<LogEventLevel>
	{
		public static MinimumLevelConfiguration Instance { get; } = new MinimumLevelConfiguration();
		MinimumLevelConfiguration() : base( o => LogEventLevel.Information ) {}
	}

	public class ProfilerLevelConfiguration : CachedParameterizedScope<LogEventLevel>
	{
		public static ProfilerLevelConfiguration Instance { get; } = new ProfilerLevelConfiguration();
		ProfilerLevelConfiguration() : base( o => LogEventLevel.Debug ) {}
	}
}
