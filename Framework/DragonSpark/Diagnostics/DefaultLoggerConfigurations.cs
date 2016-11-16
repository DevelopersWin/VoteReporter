using DragonSpark.Commands;
using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Serilog.Core;
using System;
using LoggerConfiguration = Serilog.LoggerConfiguration;

namespace DragonSpark.Diagnostics
{
	sealed class DefaultLoggerConfigurations : ItemSource<IAlteration<LoggerConfiguration>>
	{
		readonly static IAlteration<LoggerConfiguration> LogContext = EnrichFromLogContextCommand.Default.ToAlteration();

		public static DefaultLoggerConfigurations Default { get; } = new DefaultLoggerConfigurations();
		DefaultLoggerConfigurations() : base( LogContext, FormatterConfiguration.Default, ControllerAlteration.Implementation, ApplicationAssemblyAlteration.Default ) {}

		sealed class ControllerAlteration : AlterationBase<LoggerConfiguration>
		{
			public static ControllerAlteration Implementation { get; } = new ControllerAlteration();
			ControllerAlteration() : this( LoggingController.Default.Get ) {}

			readonly Func<LoggingLevelSwitch> controller;

			ControllerAlteration( Func<LoggingLevelSwitch> controller )
			{
				this.controller = controller;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.MinimumLevel.ControlledBy( controller() );
		}
	}
	
}