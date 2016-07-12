namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class ProfileAttributeTests
	{
		/*[Fact]
		public void Logger()
		{
			var history = new LoggerHistorySink();
			var level = new LoggingLevelSwitch();
			using ( MethodBase.GetCurrentMethod().AsCurrentContext( history, level ) )
			{
				Assert.Empty( history.Events );

				var tester = new PerformanceTester();
				tester.PlatformSpecific();

				Assert.Empty( history.Events );

				level.MinimumLevel = LogEventLevel.Debug;

				tester.PlatformSpecific();

				var messages = LogEventMessageFactory.Instance.Create( history.Events );

				Assert.NotEmpty( messages );
				Assert.Equal( 2, messages.Length );
				Assert.Contains( TimerEvents.Instance.Starting, messages.First() );
				Assert.Contains( TimerEvents.Instance.Completed, messages.Last() );

				Assert.All( messages, s =>
				{
					Assert.Contains( nameof(PerformanceTester.PlatformSpecific), s );
					Assert.Contains( "CPU time: ", s );
				} );
				Assert.Contains( TimerEvents.Instance.Starting, messages.First() );
				Assert.Contains( TimerEvents.Instance.Completed, messages.Last() );
			}
		}

		class PerformanceTester
		{
			[Profile]
			public void PlatformSpecific() => Thread.Sleep( 1 );
		}*/
	}
}
