using Serilog;
using Serilog.Configuration;
using System.IO;

namespace DragonSpark.Diagnostics.Configurations
{
	public class AddTextWriterConfiguration : AddSinkConfigurationBase
	{
		const string Template = "{Timestamp} [{Level}] {Message}{NewLine}{Exception}";

		public AddTextWriterConfiguration() : this( Template ) {}

		public AddTextWriterConfiguration( string outputTemplate ) : this( new StringWriter(), outputTemplate ) {}

		public AddTextWriterConfiguration( TextWriter writer, string outputTemplate = Template )
		{
			Writer = writer;
			OutputTemplate = outputTemplate;
		}

		public TextWriter Writer { get; set; }

		public string OutputTemplate { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.TextWriter( Writer, RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}
}