using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using Serilog.Events;


namespace DragonSpark.Testing.Framework.Setup
{
	public class MinimumLevel : CommandAttributeBase
	{
		public MinimumLevel( LogEventLevel level ) : base( MinimumLevelConfiguration.Instance.Configured( level ).Cast<AutoData>().WithPriority( Priority.BeforeNormal ) ) {}
	}
}