using PostSharp.Patterns.Contracts;
using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public class ReadFromSettingsConfiguration : ReadFromConfigurationBase
	{
		[Required]
		public ILoggerSettings Settings { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.Settings( Settings );
	}
}