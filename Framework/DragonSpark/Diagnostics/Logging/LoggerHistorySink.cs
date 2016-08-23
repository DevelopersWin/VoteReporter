using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class LoggerHistorySink : ILoggerHistory
	{
		readonly ConcurrentStack<LogEvent> source = new ConcurrentStack<LogEvent>();

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events => source.ToArray().Reverse().ToArray();

		public void Emit( LogEvent logEvent ) => source.Push( logEvent );
	}
}