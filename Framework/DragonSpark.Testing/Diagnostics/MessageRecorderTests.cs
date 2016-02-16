using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Testing.Framework.Setup;
using Serilog;
using Serilog.Events;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class MessageRecorderTests
	{
		[Theory, AutoData]
		public void Message( RecordingLogEventSink sut, string message, Priority priority )
		{
			var logger = new LoggerConfiguration().WriteTo.Sink( sut ).CreateLogger();

			logger.Information( message, priority );

			var item = sut.Events.Only();
			Assert.NotNull( item );

			Assert.Equal( LogEventLevel.Information, item.Level );
		}

		[Theory, AutoData]
		public void Fatal( RecordingLogEventSink sut, string message, FatalApplicationException error )
		{
			var logger = new LoggerConfiguration().WriteTo.Sink( sut ).CreateLogger();

			logger.Fatal( error, message );

			var item = sut.Events.Only();
			Assert.NotNull( item );

			Assert.Equal( LogEventLevel.Fatal, item.Level );
			Assert.Contains( error.GetType().Name, item.RenderMessage() );
		}
	}
}