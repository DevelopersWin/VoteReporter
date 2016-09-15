using DragonSpark.Sources.Parameterized;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Aspects.Validation;

namespace DragonSpark.Diagnostics.Logging
{
	[ApplyAutoValidation]
	public class PurgeLoggerMessageHistoryCommand : PurgeLoggerHistoryCommand<string>
	{
		readonly static Func<IEnumerable<LogEvent>, ImmutableArray<string>> MessageFactory = LogEventMessageFactory.Default.ToSourceDelegate();

		public static PurgeLoggerMessageHistoryCommand Default { get; } = new PurgeLoggerMessageHistoryCommand();
		PurgeLoggerMessageHistoryCommand() : this( LoggingHistory.Default.Get ) {}

		public PurgeLoggerMessageHistoryCommand( Func<ILoggerHistory> historySource ) : base( historySource, MessageFactory ) {}
	}
}