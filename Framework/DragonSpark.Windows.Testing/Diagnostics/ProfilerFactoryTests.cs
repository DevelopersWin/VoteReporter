using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DragonSpark.Diagnostics.Logger.Categories;
using Xunit;
using Xunit.Abstractions;
using Debug = DragonSpark.Diagnostics.Logger.Categories.Debug;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class ProfilerFactoryTests : TestBase
	{
		public ProfilerFactoryTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void TracerFactoryWorksAsExpected()
		{
			var history = new LoggerHistorySink();
			var message = "Hello World";
			var logEvent = new LogEvent( DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplateParser().Parse( message ), new LogEventProperty[0] );
			history.Emit( logEvent );

			Assert.Contains( logEvent, history.Events );
			var lines = new List<string>();
			var listeners = new List<TraceListener>();
			TraceListener only;
			var logger = new RecordingLoggerFactory( history, new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Debug } ).Create();
			using ( new DragonSpark.Testing.Framework.Diagnostics.ProfilerFactory<Debug>( lines.Add, logger, history, listeners ).Create( MethodBase.GetCurrentMethod() ) )
			{
				only = listeners.Only();
				Assert.Equal( 1, listeners.Count );
				Assert.Contains( only, Trace.Listeners.Cast<TraceListener>() );

				Assert.DoesNotContain( logEvent, history.Events );
				Assert.NotEmpty( lines );
				Assert.Contains( message, lines.Only() );
				Assert.NotEmpty( history.Events );
				
			}

			Assert.Empty( listeners );
			Assert.DoesNotContain( only, Trace.Listeners.Cast<TraceListener>() );
			Assert.Empty( history.Events );
			Assert.Equal( 3, lines.Count );
		}
	}
}