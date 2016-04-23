using System;
using System.Diagnostics;
using System.Windows.Markup;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using Serilog.Events;

namespace DragonSpark.Diagnostics
{
	[ContentProperty( nameof(Profiler) )]
	public class Configuration : ConfigurationBase
	{
		public ProfilerConfiguration Profiler { get; set; } = new ProfilerConfiguration();

		[Default( LogEventLevel.Information )]
		public LogEventLevel MinimumLevel { get; set; }
	}

	public class ProfilerConfiguration
	{
		[Default( typeof(ProfilerFactory) )]
		public Type FactoryType { get; set; }

		[Default( LogEventLevel.Debug )]
		public LogEventLevel Level { get; set; }
	}
}
