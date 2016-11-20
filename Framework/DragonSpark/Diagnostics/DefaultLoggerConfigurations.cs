using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Sources;
using System.Collections.Generic;

namespace DragonSpark.Diagnostics
{
	public sealed class DefaultLoggerConfigurations : ItemSourceBase<ILoggingConfiguration>
	{
		public static DefaultLoggerConfigurations Default { get; } = new DefaultLoggerConfigurations();
		DefaultLoggerConfigurations() {}

		protected override IEnumerable<ILoggingConfiguration> Yield()
		{
			yield return EnrichFromLogContextConfiguration.Default;
			yield return FormatterConfiguration.Default;
			yield return ApplicationAssemblyConfiguration.Default;
			yield return new MinimumLevelSwitchConfiguration( LoggingController.Default.Get() );
		}
	}
}