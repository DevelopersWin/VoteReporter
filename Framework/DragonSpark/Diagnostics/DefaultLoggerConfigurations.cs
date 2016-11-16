using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;
using LoggerConfiguration = Serilog.LoggerConfiguration;

namespace DragonSpark.Diagnostics
{
	sealed class DefaultLoggerConfigurations : ItemSource<ILoggingConfiguration>
	{
		public static DefaultLoggerConfigurations Default { get; } = new DefaultLoggerConfigurations();
		DefaultLoggerConfigurations() : base( 
			EnrichFromLogContextConfiguration.Default, 
			FormatterConfiguration.Default, 
			ApplicationAssemblyAlteration.Default ) {}

		protected override IEnumerable<ILoggingConfiguration> Yield() => 
			base.Yield().Append( new MinimumLevelSwitchConfiguration( LoggingController.Default.Get() ) );
	}
	
}