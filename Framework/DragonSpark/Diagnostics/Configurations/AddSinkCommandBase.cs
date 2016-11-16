using Serilog.Configuration;
using Serilog.Events;
using System;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class AddSinkCommandBase : ConfigureLoggerBase<LoggerSinkConfiguration>
	{
		protected AddSinkCommandBase() : this( LogEventLevel.Verbose ) {}

		protected AddSinkCommandBase( LogEventLevel restrictedToMinimumLevel ) : base( configuration => configuration.WriteTo )
		{
			RestrictedToMinimumLevel = restrictedToMinimumLevel;
		}

		public IFormatProvider FormatProvider { get; set; }

		public LogEventLevel RestrictedToMinimumLevel { get; set; }
	}
}