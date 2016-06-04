using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class ProfilerFactoryTests : TestCollectionBase
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
			var logger = new RecordingLoggerFactory( history, new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Debug } ).Create();

			var listener = logger.Get( Tracing.Listener );
			Assert.NotNull( listener );
			Assert.Same( listener, logger.Get( Tracing.Listener ) );

			Assert.DoesNotContain( listener, Trace.Listeners.Cast<TraceListener>() );
			using ( MethodBase.GetCurrentMethod().Profile( lines.Add, history, logger ) )
			{
				Assert.Contains( listener, Trace.Listeners.Cast<TraceListener>() );

				Assert.DoesNotContain( logEvent, history.Events );
				Assert.NotEmpty( lines );
				Assert.Contains( message, lines.Only() );
				Assert.NotEmpty( history.Events );
			}

			Assert.DoesNotContain( listener, Trace.Listeners.Cast<TraceListener>() );

			Assert.Empty( history.Events );
			Assert.Equal( 3, lines.Count );
		}
	}
}