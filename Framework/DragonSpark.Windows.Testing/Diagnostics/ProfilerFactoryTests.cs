using DragonSpark.Testing.Framework;
using Xunit.Abstractions;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class ProfilerFactoryTests : TestCollectionBase
	{
		public ProfilerFactoryTests( ITestOutputHelper output ) : base( output ) {}

		/*[Fact]
		public void TracerFactoryWorksAsExpected()
		{
			var currentMethod = MethodBase.GetCurrentMethod();

			var history = LoggingHistory.Instance.Get( currentMethod );
			var message = "Hello World";
			var logEvent = new LogEvent( DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplateParser().Parse( message ), new LogEventProperty[0] );
			history.Emit( logEvent );

			Assert.Contains( logEvent, history.Events );
			var lines = new List<string>();
			LoggingController.Instance.Get( currentMethod ).MinimumLevel = LogEventLevel.Debug;
			var logger = Logging.Instance.Get( currentMethod );

			var listener = Tracing.Listener.Get( logger );
			Assert.NotNull( listener );
			Assert.Same( listener, Tracing.Listener.Get( logger ) );

			Assert.DoesNotContain( listener, Trace.Listeners.Cast<TraceListener>() );
			
			/*using ( currentMethod.Trace( lines.Add ) )
			{
				Assert.Contains( listener, Trace.Listeners.Cast<TraceListener>() );

				Assert.DoesNotContain( logEvent, history.Events );
				Assert.NotEmpty( lines );
				Assert.Contains( message, lines.Only() );
				Assert.NotEmpty( history.Events );
			}#1#

			Assert.DoesNotContain( listener, Trace.Listeners.Cast<TraceListener>() );

			Assert.Empty( history.Events );
			Assert.Equal( 3, lines.Count );
		}*/
	}
}