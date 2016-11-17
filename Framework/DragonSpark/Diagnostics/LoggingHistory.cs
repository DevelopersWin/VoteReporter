using DragonSpark.Sources.Scopes;
using Serilog.Events;
using System.Collections.Generic;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggingHistory : SingletonScope<ILoggerHistory>, ILoggerHistory
	{
		public static ILoggerHistory Default { get; } = new LoggingHistory();
		LoggingHistory() : base( () => new LoggerHistorySink() ) {}

		public void Emit( LogEvent logEvent ) => Get().Emit( logEvent );
		public IEnumerable<LogEvent> Events => Get().Events;
		public void Clear() => Get().Clear();
	}
}