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
			var sinkOne = host.GetExport<RecordingLogEventSink>();
			var sinkTwo = host.GetExport<RecordingLogEventSink>();
			Assert.Same( sinkOne, sinkTwo );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );
		}
	}
}