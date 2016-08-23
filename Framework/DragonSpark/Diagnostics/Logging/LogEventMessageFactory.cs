using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources.Parameterized;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class LogEventMessageFactory : ParameterizedSourceBase<IEnumerable<LogEvent>, ImmutableArray<string>>
	{
		readonly static Func<LogEvent, string> Text = LogEventTextFactory.Default.ToSourceDelegate();
		public static LogEventMessageFactory Default { get; } = new LogEventMessageFactory();
		LogEventMessageFactory() {}

		public override ImmutableArray<string> Get( IEnumerable<LogEvent> parameter ) => parameter
			.OrderBy( line => line.Timestamp )
			.Select( Text )
			.ToImmutableArray();
	}
}