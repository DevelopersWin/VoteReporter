using DragonSpark.Diagnostics.Logging.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using Serilog.Events;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public class MinimumLevel : CommandAttributeBase
	{
		public MinimumLevel( LogEventLevel level ) : base( MinimumLevelConfiguration.Default.Configured( level ).Cast<AutoData>().WithPriority( Priority.BeforeNormal ) ) {}
	}
}