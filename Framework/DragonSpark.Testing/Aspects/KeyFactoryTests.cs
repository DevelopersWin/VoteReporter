using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects
{
	public class KeyFactoryTests : TestBase
	{
		public KeyFactoryTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void TracerFactoryWorksAsExpected()
		{
			var count = Trace.Listeners.Count;
			var history = new LoggerHistorySink();
			var message = "Hello World";
			var logEvent = new LogEvent( DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplateParser().Parse( message ), new LogEventProperty[0] );
			history.Emit( logEvent );

			Assert.Contains( logEvent, history.Events );
			var lines = new List<string>();
			using ( new TracerFactory( lines.Add, history ).Create() )
			{
				Assert.Equal( count + 1, Trace.Listeners.Count );

				Assert.DoesNotContain( logEvent, history.Events );
				Assert.NotEmpty( lines );
				Assert.Contains( message, lines.Only() );
				Assert.NotEmpty( history.Events );
				// Assert.Equal( lines.Count, history.Events.Count() );
			}
			Assert.Equal( count, Trace.Listeners.Count );
			Assert.Empty( history.Events );
			Assert.Equal( 3, lines.Count );
		}

		/*[Fact]
		public void Basic()
		{
			using ( var tracer = new TracerFactory( Output.WriteLine ).Create() )
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