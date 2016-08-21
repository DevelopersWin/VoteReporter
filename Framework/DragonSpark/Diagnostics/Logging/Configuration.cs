using DragonSpark.Sources;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging
{
	public class MinimumLevelConfiguration : Scope<LogEventLevel>
	{
		public static MinimumLevelConfiguration Default { get; } = new MinimumLevelConfiguration();
		MinimumLevelConfiguration() : base( () => LogEventLevel.Information ) {}
	}

	public class ProfilerLevelConfiguration : Scope<LogEventLevel>
	{
		public static ProfilerLevelConfiguration Default { get; } = new ProfilerLevelConfiguration();
		ProfilerLevelConfiguration() : base( () => LogEventLevel.Debug ) {}
	}
}
