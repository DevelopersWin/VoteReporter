using DragonSpark.Diagnostics.Configurations;
using JetBrains.Annotations;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace DragonSpark.Windows.Diagnostics
{
	public class AddConsoleSinkConfiguration : AddSinkConfigurationBase
	{
		public AddConsoleSinkConfiguration() : this( DragonSpark.Diagnostics.Defaults.Template, LogEventLevel.Verbose ) {}

		public AddConsoleSinkConfiguration( [NotEmpty]string outputTemplate, LogEventLevel restrictedToMinimumLevel ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
		}

		[NotEmpty, UsedImplicitly]
		public string OutputTemplate { [return: NotEmpty]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration )
			=> configuration.ColoredConsole( RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}
}