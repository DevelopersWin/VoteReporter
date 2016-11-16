using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerHistorySink : ILoggerHistory
	{
		readonly IList<LogEvent> source = new List<LogEvent>();

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events => source.Hide();

		public void Emit( LogEvent logEvent ) => source.Add( logEvent );
	}
}