using DragonSpark.Diagnostics.Logging;
using DragonSpark.Sources;
using Ploeh.AutoFixture.Xunit2;
using Serilog.Core;
using Serilog.Events;
using System.Linq;
using Xunit;
using Logger = DragonSpark.Diagnostics.Logging.Logger;

namespace DragonSpark.Testing.Activation
{
	public class RecordingLoggerConfigurationCommandTests
	{
		[Fact]
		public void BasicContext()
		{
			var level = MinimumLevelConfiguration.Instance;
			var controller = LoggingController.Instance;

			var first = controller.Get();
			Assert.Same( first, controller.Get() );

			Assert.Equal( LogEventLevel.Information, first.MinimumLevel );

			const LogEventLevel assigned = LogEventLevel.Debug;
			level.Assign( assigned );
			controller.Assign( Factory.Global( () => new LoggingLevelSwitch( MinimumLevelConfiguration.Instance.Get() ) ) );

			var second = controller.Get();
			Assert.NotSame( first, second );
			Assert.Same( second, controller.Get() );

			Assert.Equal( assigned, second.MinimumLevel );
		}

		[Theory, AutoData]
		void VerifyHistory( object context, string message )
		{
			var history = LoggingHistory.Instance.Get();
			Assert.Empty( history.Events );
			Assert.Same( history, LoggingHistory.Instance.Get() );

			var logger = Logger.Instance.Get( context );
			Assert.Empty( history.Events );
			logger.Information( "Hello World! {Message}", message );
			Assert.Single( history.Events, item => item.RenderMessage().Contains( message ) );

			logger.Debug( "Hello World! {Message}", message );
			Assert.Single( history.Events );
			LoggingController.Instance.Get().MinimumLevel = LogEventLevel.Debug;

			logger.Debug( "Hello World! {Message}", message );
			Assert.Equal( 2, history.Events.Count() );
		}
	}
}
