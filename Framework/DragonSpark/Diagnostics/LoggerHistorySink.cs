using DragonSpark.Extensions;
using Serilog.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DragonSpark.Activation;

namespace DragonSpark.Diagnostics
{
	public class LoggerHistorySink : ILoggerHistory
	{
		readonly IList<LogEvent> source = new Collection<LogEvent>();
		readonly IReadOnlyCollection<LogEvent> events;

		public LoggerHistorySink()
		{
			events = new ReadOnlyCollection<LogEvent>( source );
		}

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events => events;

		public virtual void Emit( LogEvent logEvent ) => source.Ensure( logEvent );
	}

	public class LogEventMessageFactory : FactoryBase<IEnumerable<LogEvent>, string[]>
	{
		public static LogEventMessageFactory Instance { get; } = new LogEventMessageFactory();

		protected override string[] CreateItem( IEnumerable<LogEvent> parameter ) => parameter.OrderBy( line => line.Timestamp ).Select( line => line.RenderMessage() ).ToArray();
	}
}