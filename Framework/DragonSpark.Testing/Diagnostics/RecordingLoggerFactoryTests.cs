using DragonSpark.Diagnostics;
using DragonSpark.Testing.Objects.Setup;
using Serilog;
using System.Composition;
using DragonSpark.Testing.Framework;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Diagnostics
{
	public class RecordingLoggerFactoryTests : Tests
	{
		public RecordingLoggerFactoryTests( ITestOutputHelper output ) : base( output ) {}

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