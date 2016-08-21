using DragonSpark.Diagnostics.Logging;
using DragonSpark.Sources.Parameterized;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerHistorySink : ILoggerHistory
	{
		readonly ConcurrentStack<LogEvent> source = new ConcurrentStack<LogEvent>();

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events => source.ToArray().Reverse().ToArray();

		public void Emit( LogEvent logEvent ) => source.Push( logEvent );
	}

	public sealed class LogEventTextFactory : ParameterizedSourceBase<LogEvent, string>
	{
		public static LogEventTextFactory Default { get; } = new LogEventTextFactory();
		LogEventTextFactory( string template = "{Timestamp:HH:mm:ss:fff} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}" ) : this( new MessageTemplateTextFormatter( template, null ) ) {}

		readonly MessageTemplateTextFormatter formatter;

		public LogEventTextFactory( MessageTemplateTextFormatter formatter )
		{
			this.formatter = formatter;
		}

		public override string Get( LogEvent parameter )
		{
			var writer = new StringWriter();
			formatter.Format( parameter, writer );
			var result = writer.ToString().Trim();
			return result;
		}
	}

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