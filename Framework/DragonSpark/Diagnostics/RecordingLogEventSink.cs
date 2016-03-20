using DragonSpark.Extensions;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;

namespace DragonSpark.Diagnostics
{
	[Export, Shared]
	public class RecordingLogEventSink : ILogEventSink
	{
		readonly IList<LogEvent> source = new Collection<LogEvent>();
		readonly IReadOnlyCollection<LogEvent> events;

		public RecordingLogEventSink()
		{
			events = new ReadOnlyCollection<LogEvent>( source );
		}

		public LogEvent[] Purge() => source.Purge();
		
		// protected override void OnLog( Message message ) => source.Add( message );

		public IEnumerable<LogEvent> Events => events;

		public virtual void Emit( LogEvent logEvent ) => source.Contains( logEvent).IsFalse( () => source.Add( logEvent ) );
	}

	[Export( typeof(Serilog.Core.LoggingLevelSwitch) ), Shared]
	public class LoggingLevelSwitch : Serilog.Core.LoggingLevelSwitch
	{}
}