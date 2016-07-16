using DragonSpark.Configuration;
using Serilog.Events;

namespace DragonSpark.Diagnostics
{
	public class MinimumLevelConfiguration : StructuredParameterizedConfiguration<LogEventLevel>
	{
		public static MinimumLevelConfiguration Instance { get; } = new MinimumLevelConfiguration();
		MinimumLevelConfiguration() : base( o => LogEventLevel.Information ) {}
	}

	/*public class MinimumLevelConfiguration : DeclarativeConfigurationStoreBase<LogEventLevel>
	{
		public MinimumLevelConfiguration() : base( LogEventLevel.Information ) {}
	}*/

	/*public class ProfilerFactoryConfiguration : ConfigurationSource<MethodBase, IProfiler>
	{
		public static IParameterizedConfiguration<MethodBase, IProfiler> Instance { get; } = new ProfilerFactoryConfiguration();
		ProfilerFactoryConfiguration() : base( m => new ProfilerFactory().Create( m ) ) {}
	}*/

	public class ProfilerLevelConfiguration : StructuredParameterizedConfiguration<LogEventLevel>
	{
		public static ProfilerLevelConfiguration Instance { get; } = new ProfilerLevelConfiguration();
		ProfilerLevelConfiguration() : base( o => LogEventLevel.Debug ) {}
	}

	/*[ContentProperty( nameof(Profiler) )]
	public class Configuration : ExecutionContextConfigurationBase
	{
		public ProfilerConfiguration Profiler { get; set; } = new ProfilerConfiguration();

		[Default( LogEventLevel.Information )]
		public LogEventLevel MinimumLevel { get; set; }
	}

	public class ProfilerConfiguration
	{
		[Default(  )]
		public Type FactoryType { get; set; }

		[Default(  )]
		public LogEventLevel Level { get; set; }
	}*/
}
