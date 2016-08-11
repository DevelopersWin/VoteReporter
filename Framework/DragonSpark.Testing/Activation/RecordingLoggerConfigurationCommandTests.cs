using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Runtime.Sources;
using Ploeh.AutoFixture.Xunit2;
using Serilog.Events;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class RecordingLoggerConfigurationCommandTests
	{
		[Fact]
		public void BasicContext()
		{
			var level = MinimumLevelConfiguration.Instance;
			var controller = LoggingController.Instance;

			var one = new object();
			var first = controller.Get( one );
			Assert.Same( first, controller.Get( one ) );

			Assert.Equal( LogEventLevel.Information, first.MinimumLevel );

			const LogEventLevel assigned = LogEventLevel.Debug;
			level.Assign( assigned );

			var two = new object();
			var second = controller.Get( two );
			Assert.NotSame( first, second );
			Assert.Same( second, controller.Get( two ) );

			Assert.Equal( assigned, second.MinimumLevel );
		}

		[Theory, AutoData]
		void VerifyHistory( object context, string message )
		{
			var history = LoggingHistory.Instance.Get( context );
			Assert.Empty( history.Events );
			Assert.Same( history, LoggingHistory.Instance.Get( context ) );

			var logger = Logging.Instance.Get( context );
			Assert.Empty( history.Events );
			logger.Information( "Hello World! {Message}", message );
			Assert.Single( history.Events, item => item.RenderMessage().Contains( message ) );

			logger.Debug( "Hello World! {Message}", message );
			Assert.Single( history.Events );
			var level = LoggingController.Instance.Get( context );
			level.MinimumLevel = LogEventLevel.Debug;

			logger.Debug( "Hello World! {Message}", message );
			Assert.Equal( 2, history.Events.Count() );
		}
	}
}
