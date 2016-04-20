using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger.Categories;
using DragonSpark.Runtime;
using DragonSpark.Setup.Commands;
using Serilog.Events;
using System;
using System.Windows.Markup;

namespace DragonSpark.Configuration
{
	[ContentProperty( nameof(Parameter) )]
	public class InitializeFrameworkConfigurationCommand : ServicedCommand<ConfigureFrameworkCommand, FrameworkConfiguration> {}

	public class FrameworkConfiguration
	{
		public static FrameworkConfiguration Current { get; private set; } = new FrameworkConfiguration();

		public static void Initialize( FrameworkConfiguration configuration ) => Current = configuration;

		[Default( true )]
		public bool EnableMethodCaching { get; set; }

		public Diagnostics Diagnostics { get; set; } = new Diagnostics();
	}

	public class Diagnostics
	{
		[Default( typeof(ProfilerFactory<Debug>) )]
		public Type ProfilerFactoryType { get; set; }

		[Default( LogEventLevel.Information )]
		public LogEventLevel Level { get; set; }
	}

	public class ConfigureFrameworkCommand : Command<FrameworkConfiguration>
	{
		protected override void OnExecute( FrameworkConfiguration parameter ) => FrameworkConfiguration.Initialize( parameter );
	}
}