using System;
using Serilog.Configuration;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public abstract class AddSinkCommand : LoggerConfigurationCommandBase<LoggerSinkConfiguration>
	{
		protected AddSinkCommand() : this( LogEventLevel.Verbose ) {}

		protected AddSinkCommand( LogEventLevel restrictedToMinimumLevel ) : base( configuration => configuration.WriteTo )
		{
			RestrictedToMinimumLevel = restrictedToMinimumLevel;
		}

		public IFormatProvider FormatProvider { get; set; }

		public LogEventLevel RestrictedToMinimumLevel { get; set; }
	}
}