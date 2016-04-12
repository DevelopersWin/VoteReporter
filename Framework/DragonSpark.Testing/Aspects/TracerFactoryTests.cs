using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects
{
	public class TracerFactoryTests : TestBase
	{
		public TracerFactoryTests( ITestOutputHelper output ) : base( output ) {}

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
			using ( new TracingProfilerFactory( lines.Add, history ).Create() )
			{
				only = listeners.Only();
				Assert.Equal( 1, listeners.Count );
				Assert.Contains( only, Trace.Listeners.Cast<TraceListener>() );

				Assert.DoesNotContain( logEvent, history.Events );
				Assert.NotEmpty( lines );
				Assert.Contains( message, lines.Only() );
				Assert.NotEmpty( history.Events );
				// Assert.Equal( lines.Count, history.Events.Count() );
			}

			Assert.Empty( listeners );
			Assert.DoesNotContain( only, Trace.Listeners.Cast<TraceListener>() );
			Assert.Empty( history.Events );
			Assert.Equal( 3, lines.Count );
		}

		/*[Fact]
		public void Basic()
		{
			using ( var tracer = new ProfilerFactory( Output.WriteLine ).Create() )
			{
				tracer.Mark( "Enter" );
				var objects = new[] { new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object(), new object() };


				for ( int i = 0; i < 100000; i++ )
				{
					HashCodeKeyFactory.Instance.Create( objects );
				}

				tracer.Mark( "HashCodeKeyFactory" );

				for ( int i = 0; i < 100000; i++ )
				{
					KeyFactory.Instance.Create( objects );
				}

				tracer.Mark( "KeyFactory" );

				/*for ( int i = 0; i < 100000; i++ )
				{
					JoinFactory.Instance.Create( objects );
				}

				tracer.Profiler.Mark( "JoinFactory" );

				for ( int i = 0; i < 100000; i++ )
				{
					Builder.Instance.Create( objects );
				}

				tracer.Profiler.Mark( "Builder" );#1#
			}
		}*/
	}
}