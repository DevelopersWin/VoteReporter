using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using Serilog.Core;
using Serilog.Events;
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

				Assert.Empty( history.Events );

				level.MinimumLevel = LogEventLevel.Debug;

				tester.Perform();

				Assert.NotEmpty( history.Events );
			}
		}

		class PerformanceTester
		{
			[Aspects.Profile]
			public void Perform() => Thread.Sleep( 50 );
		}
	}
}
