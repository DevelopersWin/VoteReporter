using DragonSpark.Extensions;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DragonSpark.Diagnostics
{
	public class RecordingLogEventSink : ILogEventSink
	{
		readonly IList<LogEvent> source = new Collection<LogEvent>();
		readonly IReadOnlyCollection<LogEvent> events;

		public RecordingLogEventSink()
		{
			events = new ReadOnlyCollection<LogEvent>( source );
		}

		public LogEvent[] Purge() => source.Purge();

		public IEnumerable<LogEvent> Events => events;

		public virtual void Emit( LogEvent logEvent ) => source.Contains( logEvent).IsFalse( () => source.Add( logEvent ) );
	}
}