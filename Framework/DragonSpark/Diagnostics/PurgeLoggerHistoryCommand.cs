using JetBrains.Annotations;
using Serilog.Events;
using System.Collections.Immutable;

namespace DragonSpark.Diagnostics
{
	public sealed class PurgeLoggerHistoryCommand : PurgeLoggerHistoryCommandBase<LogEvent>
	{
		public static PurgeLoggerHistoryCommand Default { get; } = new PurgeLoggerHistoryCommand();
		PurgeLoggerHistoryCommand() : this( LoggingHistory.Default ) {}

		[UsedImplicitly]
		public PurgeLoggerHistoryCommand( ILoggerHistory history ) : base( history, events => events.ToImmutableArray() ) {}
	}
}