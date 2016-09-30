using DragonSpark.Diagnostics.Logging;
using DragonSpark.Diagnostics.Logging.Configurations;
using PostSharp.Patterns.Contracts;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.Diagnostics;
using System.IO;

namespace DragonSpark.Windows.Diagnostics
{
	public sealed class AddTraceSinkCommand : AddSinkCommand
	{
		public AddTraceSinkCommand() : this( Defaults.Template, LogEventLevel.Verbose ) {}

		public AddTraceSinkCommand( string outputTemplate, LogEventLevel restrictedToMinimumLevel ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
		}

		[NotEmpty]
		public string OutputTemplate { [return: NotEmpty]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => 
			configuration.Sink( new TraceSink( new MessageTemplateTextFormatter( OutputTemplate, FormatProvider ) ), RestrictedToMinimumLevel );
	}

	sealed class TraceSink : ILogEventSink
	{
		readonly ITextFormatter textFormatter;

		public TraceSink( ITextFormatter textFormatter )
		{
			this.textFormatter = textFormatter;
		}

		public void Emit( LogEvent logEvent )
		{
			var stringWriter = new StringWriter();
			textFormatter.Format( logEvent, stringWriter );
			var message = stringWriter.ToString().Trim();
			Trace.WriteLine( message );
		}
	}
}