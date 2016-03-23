using DragonSpark.Extensions;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DragonSpark.Activation.FactoryModel;

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

		public virtual void Emit( LogEvent logEvent ) => source.Contains( logEvent ).IsFalse( () => source.Add( logEvent ) );
	}

	public class PurgingEventFactory : FactoryBase<RecordingLogEventSink, string[]>
	{
		public static PurgingEventFactory Instance { get; } = new PurgingEventFactory();

		protected override string[] CreateItem( RecordingLogEventSink parameter ) => parameter.Purge().OrderBy( line => line.Timestamp ).Select( line => line.RenderMessage() ).ToArray();
	}
}