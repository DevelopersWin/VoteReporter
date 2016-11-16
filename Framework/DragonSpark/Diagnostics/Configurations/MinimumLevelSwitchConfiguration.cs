using PostSharp.Patterns.Contracts;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Configurations
{
	public class MinimumLevelSwitchConfiguration : MinimumLevelConfigurationBase
	{
		public MinimumLevelSwitchConfiguration() : this( LogEventLevel.Information ) {}

		public MinimumLevelSwitchConfiguration( LogEventLevel level ) : this( new LoggingLevelSwitch( level ) ) {}

		public MinimumLevelSwitchConfiguration( LoggingLevelSwitch controller )
		{
			Controller = controller;
		}

		[Required]
		public LoggingLevelSwitch Controller { [return: Required]get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.ControlledBy( Controller );
	}
}