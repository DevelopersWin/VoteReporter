using DragonSpark.Diagnostics.Configurations;
using PostSharp.Patterns.Contracts;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace DragonSpark.Windows.Diagnostics
{
	public sealed class AddTraceSinkConfiguration : AddSinkConfigurationBase
	{
		public AddTraceSinkConfiguration() : this( DragonSpark.Diagnostics.Defaults.Template, LogEventLevel.Verbose ) {}

		public AddTraceSinkConfiguration( string outputTemplate, LogEventLevel restrictedToMinimumLevel ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
		}

		[NotEmpty]
		public string OutputTemplate { [return: NotEmpty]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => 
			configuration.Sink( new TraceSink( new MessageTemplateTextFormatter( OutputTemplate, FormatProvider ) ), RestrictedToMinimumLevel );
	}
}