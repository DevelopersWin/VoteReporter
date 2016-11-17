using DragonSpark.Diagnostics;
using DragonSpark.Sources.Parameterized;
using Serilog;
using System.Collections.Immutable;
using Xunit;
using Defaults = DragonSpark.Diagnostics.Defaults;

namespace DragonSpark.Testing.Diagnostics
{
	public class LoggerConfigurationSourceTests
	{
		[Fact]
		public void Verify()
		{
			var history = LoggingHistory.Default;
			var sink = new LoggerHistorySink();
			LoggerConfigurationSource.Default.Assign( () => new LoggerConfiguration().WriteTo.Sink( sink ) );
			//LoggerFactory.Factory.Instance.Assign( configuration => configuration.CreateLogger() );

			var empty = CreateLogger.Instance.Get( new LoggerConfiguration() );
			Assert.NotNull( empty );


			var system = SystemLogger.Default.Get();
			system.Information( "Hello World!" );

			Assert.Empty( sink.Events );
			var events = history.Events.ToImmutableHashSet();
			Assert.Single( events );

			var logger = Logger.Default.GetDefault();
			var source = Defaults.Logger();
			Assert.Same( logger, source );

			Assert.Empty( history.Events );
			var set = sink.Events.ToImmutableHashSet();
			Assert.Single( set );
			Assert.Equal( events, set );

			Assert.NotSame( system, logger );

			var one = SystemLogger.Default.Get();
			var two = SystemLogger.Default.Get();
			Assert.Same( one, two );
			Assert.Same( logger, one );
		}
	}
}