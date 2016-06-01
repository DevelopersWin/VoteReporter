using DragonSpark.Diagnostics;
using PostSharp.Patterns.Threading;
using Serilog.Events;

namespace DragonSpark.Windows.Testing.TestObjects.Modules
{
	class MockLoggerHistorySink : LoggerHistorySink
	{
		[ExplicitlySynchronized]
		public string LastMessage { get; private set; }

		[ExplicitlySynchronized]
		public string LastMessageCategory;

		public override void Emit( LogEvent logEvent )
		{
			base.Emit( logEvent );
			LastMessage = logEvent.RenderMessage();
			LastMessageCategory = logEvent.Level.ToString();
		}
	}
}