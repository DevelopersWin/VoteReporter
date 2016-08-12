using DragonSpark.Activation.Sources;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using Serilog.Events;


namespace DragonSpark.Testing.Framework.Setup
{
	public class MinimumLevel : CommandAttributeBase
	{
		public MinimumLevel( LogEventLevel level ) : base( MinimumLevelConfiguration.Instance.Configured( level ).Cast<AutoData>().WithPriority( Priority.BeforeNormal ) ) {}
	}
}