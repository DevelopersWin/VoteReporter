using JetBrains.Annotations;
using Serilog;
using Serilog.Configuration;
using System.IO;

namespace DragonSpark.Diagnostics.Configurations
{
	public class AddTextWriterConfiguration : AddFormattableSinkConfigurationBase
	{
		public AddTextWriterConfiguration() : this( Defaults.Template ) {}

		public AddTextWriterConfiguration( string outputTemplate ) : this( new StringWriter(), outputTemplate ) {}

		public AddTextWriterConfiguration( TextWriter writer, string outputTemplate = Defaults.Template )
		{
			Writer = writer;
			OutputTemplate = outputTemplate;
		}

		[UsedImplicitly]
		public TextWriter Writer { get; set; }

		[UsedImplicitly]
		public string OutputTemplate { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.TextWriter( Writer, RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}
}