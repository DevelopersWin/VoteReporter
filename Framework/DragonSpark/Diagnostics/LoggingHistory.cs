using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using Serilog.Events;
using System.Collections.Generic;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggingHistory : DelegatedSource<ILoggerHistory>, ILoggerHistory
	{
		public static ILoggerHistory Default { get; } = new LoggingHistory();
		LoggingHistory() : base( new SingletonScope<LoggerHistorySink>( () => new LoggerHistorySink() ).Get ) {}

		public void Emit( LogEvent logEvent ) => Get().Emit( logEvent );
		public IEnumerable<LogEvent> Events => Get().Events;
		public void Clear() => Get().Clear();
	}
}