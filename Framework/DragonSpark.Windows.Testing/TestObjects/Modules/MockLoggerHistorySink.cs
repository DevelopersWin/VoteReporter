using DragonSpark.Diagnostics;
using Serilog.Events;

namespace DragonSpark.Windows.Testing.TestObjects.Modules
{
	class MockLoggerHistorySink : LoggerHistorySink
	{
		public string LastMessage { get; private set; }
		public string LastMessageCategory;

		public override void Emit( LogEvent logEvent )
		{
			base.Emit( logEvent );
			LastMessage = logEvent.RenderMessage();
			LastMessageCategory = logEvent.Level.ToString();
		}
	}
}