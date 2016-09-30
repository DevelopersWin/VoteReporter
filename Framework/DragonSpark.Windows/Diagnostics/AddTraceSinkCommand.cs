using DragonSpark.Diagnostics.Logging;
using DragonSpark.Diagnostics.Logging.Configurations;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace DragonSpark.Windows.Diagnostics
{
	public class AddTraceSinkCommand : AddSinkCommand
	{
		public AddTraceSinkCommand() : this( Defaults.Template, LogEventLevel.Verbose ) {}

		public AddTraceSinkCommand( string outputTemplate, LogEventLevel restrictedToMinimumLevel ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
		}

		[NotEmpty]
		public string OutputTemplate { [return: NotEmpty]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.Trace( RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}
}