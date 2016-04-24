using DragonSpark.Configuration;
using Serilog.Events;
using System;

namespace DragonSpark.Diagnostics
{
	public class MinimumLevelConfiguration : ConfigurationBase<LogEventLevel>
	{
		public MinimumLevelConfiguration() : base( LogEventLevel.Information ) {}
	}

	public class ProfilerFactoryConfiguration : ConfigurationBase<Type>
	{
		public ProfilerFactoryConfiguration() : base( typeof(ProfilerFactory) ) {}
	}

	public class ProfilerLevelConfiguration : ConfigurationBase<LogEventLevel>
	{
		public ProfilerLevelConfiguration() : base( LogEventLevel.Debug ) {}
	}

	/*[ContentProperty( nameof(Profiler) )]
	public class Configuration : ConfigurationBase
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
