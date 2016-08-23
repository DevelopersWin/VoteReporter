using System;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logging.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using Serilog;
using Serilog.Core;

namespace DragonSpark.Diagnostics.Logging
{
	public abstract class LoggerConfigurationSourceBase : ConfigurationSource<LoggerConfiguration>
	{
		readonly static ITransformer<LoggerConfiguration> LogContext = EnrichFromLogContextCommand.Default.ToTransformer();

		protected LoggerConfigurationSourceBase( params ITransformer<LoggerConfiguration>[] items ) : base( items.Fixed( LogContext, FormatterConfiguration.Default, ControllerTransform.Default, ApplicationAssemblyTransform.Default ) ) {}

		sealed class ControllerTransform : TransformerBase<LoggerConfiguration>
		{
			public static ControllerTransform Default { get; } = new ControllerTransform();
			ControllerTransform() : this( LoggingController.Default.Get ) {}

			readonly Func<LoggingLevelSwitch> controller;

			ControllerTransform( Func<LoggingLevelSwitch> controller )
			{
				this.controller = controller;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.MinimumLevel.ControlledBy( controller() );
		}
	}
}