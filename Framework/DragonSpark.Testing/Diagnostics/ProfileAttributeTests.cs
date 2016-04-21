using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Testing.Framework;
using Serilog.Core;
using Serilog.Events;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class ProfileAttributeTests
	{
		[Fact]
		public void Logger()
		{
			var history = new LoggerHistorySink();
			var level = new LoggingLevelSwitch();
			using ( MethodBase.GetCurrentMethod().Assign( history, level ) )
			{
				Assert.Empty( history.Events );

				var tester = new PerformanceTester();
				tester.Perform();

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
				Assert.All( newest, s =>
				{
					Assert.Contains( nameof(PerformanceTester.Perform), s );
					Assert.DoesNotContain( "CPU time: ", s );
				} );
				Assert.Contains( TimerEvents.Instance.Starting, newest.First() );
				Assert.Contains( TimerEvents.Instance.Completed, newest.Last() );
			}
		}

		class PerformanceTester
		{
			[Profile( typeof(ProfilerFactory) )]
			public void Perform() => Thread.Sleep( 1 );
		}

		[Fact]
		public void Eventing()
		{
			Assert.Null( Ambient.GetCurrent<EmitProfileEvent>() );

			var history = new LoggerHistorySink();
			using ( MethodBase.GetCurrentMethod().Assign( history, new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Debug } ) )
			{
				Assert.Empty( history.Events );

				var sut = new EventingTester();
				var item = sut.First();
				Assert.NotNull( item );
				Assert.Null( Ambient.GetCurrent<EmitProfileEvent>() );

				var messages = LogEventMessageFactory.Instance.Create( history.Events );
				Assert.Equal( 4, messages.Length );

				var events = messages.Except( messages.First().Append( messages.Last() ) ).ToArray();
				Assert.Contains( "Inside First", events.First() );
				Assert.Contains( "Inside Second", events.Last() );
			}
			Assert.Null( Ambient.GetCurrent<EmitProfileEvent>() );
		}

		class EventingTester
		{
			[Profile]
			public EmitProfileEvent First()
			{
				Profile.Event( "Inside First" );
				Second();
				return Ambient.GetCurrent<EmitProfileEvent>();
			}

			static void Second() => Profile.Event( "Inside Second" );
		}
	}
}
