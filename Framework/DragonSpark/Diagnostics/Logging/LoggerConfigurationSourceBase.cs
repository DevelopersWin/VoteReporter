using DragonSpark.Commands;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logging.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using Serilog;
using Serilog.Core;
using System;

namespace DragonSpark.Diagnostics.Logging
{
	public abstract class LoggerConfigurationSourceBase : ConfigurationSource<LoggerConfiguration>
	{
		readonly static IAlteration<LoggerConfiguration> LogContext = EnrichFromLogContextCommand.Default.ToAlteration();

		protected LoggerConfigurationSourceBase( params IAlteration<LoggerConfiguration>[] items ) : base( items.Fixed( LogContext, FormatterConfiguration.Default, ControllerAlteration.Default, ApplicationAssemblyAlteration.Default ) ) {}

		sealed class ControllerAlteration : AlterationBase<LoggerConfiguration>
		{
			public static ControllerAlteration Default { get; } = new ControllerAlteration();
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