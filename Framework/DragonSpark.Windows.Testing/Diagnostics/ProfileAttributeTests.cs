using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using Serilog.Core;
using Serilog.Events;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class ProfileAttributeTests
	{
		[Fact]
		public void Profile()
		{
			var history = new LoggerHistorySink();
			var level = new LoggingLevelSwitch();
			using ( MethodBase.GetCurrentMethod().Assign( history, level ) )
			{
				Assert.Empty( history.Events );

				var tester = new PerformanceTester();
				tester.Perform();
				tester.PlatformSpecific();

				Assert.Empty( history.Events );

				level.MinimumLevel = LogEventLevel.Debug;

				tester.Perform();

				var messages = LogEventMessageFactory.Instance.Create( history.Events );

				Assert.NotEmpty( messages );
				Assert.Equal( 2, messages.Length );
				Assert.Contains( TimerEvents.Instance.Starting, messages.First() );
				Assert.Contains( TimerEvents.Instance.Completed, messages.Last() );

				tester.Perform();


				var second = LogEventMessageFactory.Instance.Create( history.Events );

				Assert.NotEmpty( second );
				Assert.Equal( 4, second.Length );

				var newest = second.Except( messages ).ToArray();
				Assert.Equal( 2, messages.Length );

				Assert.Contains( TimerEvents.Instance.Starting, newest.First() );
				Assert.Contains( TimerEvents.Instance.Completed, newest.Last() );
				
				tester.PlatformSpecific();

				var platform = LogEventMessageFactory.Instance.Create( history.Events );
				Assert.Equal( 6, platform.Length );

				var latest = platform.Except( second ).ToArray();
				Assert.Equal( 2, latest.Length );
				Assert.All( latest, s =>
				{
					Assert.Contains( nameof(PerformanceTester.PlatformSpecific), s );
					Assert.Contains( "CPU time: ", s );
				} );
				Assert.Contains( TimerEvents.Instance.Starting, latest.First() );
				Assert.Contains( TimerEvents.Instance.Completed, latest.Last() );
			}
		}

		class PerformanceTester
		{
			[Profile]
			public void Perform() => Thread.Sleep( 1 );

			[Windows.Diagnostics.Profile]
			public void PlatformSpecific() => Thread.Sleep( 1 );
		}
	}
}
