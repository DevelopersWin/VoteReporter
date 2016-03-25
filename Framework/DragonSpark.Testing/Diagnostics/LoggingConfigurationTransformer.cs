using DragonSpark.Diagnostics;
using DragonSpark.Testing.Objects.Setup;
using Serilog;
using System.Composition;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class LoggingConfigurationTransformer
	{
		[Theory, DefaultSetup.AutoData]
		public void BasicCompose( CompositionContext host )
		{
			var sinkOne = host.GetExport<LoggerHistorySink>();
			var sinkTwo = host.GetExport<LoggerHistorySink>();
			Assert.Same( sinkOne, sinkTwo );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );
		}
	}
}