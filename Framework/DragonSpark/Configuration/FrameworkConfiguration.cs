using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Commands;
using Serilog.Events;
using System;
using System.Windows.Markup;

namespace DragonSpark.Configuration
{
	[ContentProperty( nameof(Parameter) )]
	public class InitializeFrameworkConfigurationCommand : ServicedCommand<ConfigureFrameworkCommand, FrameworkConfiguration> {}

	class CurrentFrameworkConfiguration : ExecutionContextValue<FrameworkConfiguration>
	{
		public static CurrentFrameworkConfiguration Instance { get; } = new CurrentFrameworkConfiguration();

		CurrentFrameworkConfiguration() : base( () => new FrameworkConfiguration() ) {}
	}

	public class FrameworkConfiguration
	{
		public static Func<T> Factory<T>( Func<FrameworkConfiguration, T> get ) => () => get( Current );

		public static FrameworkConfiguration Current { get; } = CurrentFrameworkConfiguration.Instance.Item;

		public static void Initialize( FrameworkConfiguration configuration ) => CurrentFrameworkConfiguration.Instance.Assign( configuration );

		[Default( true )]
		public bool EnableMethodCaching { get; set; }

		public Diagnostics Diagnostics { get; set; } = new Diagnostics();
	}

	public class Profiler
	{
		[Default( typeof(ProfilerFactory) )]
		public Type FactoryType { get; set; }

		[Default( LogEventLevel.Debug )]
		public LogEventLevel Level { get; set; }
	}

	[ContentProperty( nameof(Profiler) )]
	public class Diagnostics
	{
		public Profiler Profiler { get; set; } = new Profiler();

		[Default( LogEventLevel.Information )]
		public LogEventLevel MinimumLevel { get; set; }
	}

	public class ConfigureFrameworkCommand : Command<FrameworkConfiguration>
	{
		public static ConfigureFrameworkCommand Instance { get; } = new ConfigureFrameworkCommand();

		ConfigureFrameworkCommand() {}

		protected override void OnExecute( FrameworkConfiguration parameter ) => FrameworkConfiguration.Initialize( parameter );
	}
}