using System.IO;
using Serilog;
using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public class AddTextWriterCommand : AddSinkCommand
	{
		public AddTextWriterCommand() : this( new StringWriter(), "{Timestamp} [{Level}] {Message}{NewLine}{Exception}" ) {}

		public AddTextWriterCommand( TextWriter writer, string outputTemplate )
		{
			Writer = writer;
			OutputTemplate = outputTemplate;
		}

		public TextWriter Writer { get; set; }

		public string OutputTemplate { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.TextWriter( Writer, RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}
}