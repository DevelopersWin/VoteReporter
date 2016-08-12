using DragonSpark.Activation;
using PostSharp.Patterns.Contracts;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Diagnostics
{
	public class LoggerHistorySink : ILoggerHistory
	{
		readonly ConcurrentStack<LogEvent> source = new ConcurrentStack<LogEvent>();

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events => source.ToArray().Reverse().ToArray();

		public virtual void Emit( [Required]LogEvent logEvent ) => source.Push( logEvent );
	}

	public class LogEventTextFactory : ValidatedParameterizedSourceBase<LogEvent, string>
	{
		public static LogEventTextFactory Instance { get; } = new LogEventTextFactory();

		readonly MessageTemplateTextFormatter formatter;

		public LogEventTextFactory( string template = "{Timestamp:HH:mm:ss:fff} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}" ) : this( new MessageTemplateTextFormatter( template, null ) ) {}

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

	public sealed class LogEventMessageFactory : ValidatedParameterizedSourceBase<IEnumerable<LogEvent>, ImmutableArray<string>>
	{
		readonly static Func<LogEvent, string> Text = LogEventTextFactory.Instance.ToDelegate();
		public static LogEventMessageFactory Instance { get; } = new LogEventMessageFactory();
		LogEventMessageFactory() {}

		public override ImmutableArray<string> Get( IEnumerable<LogEvent> parameter ) => parameter
			.OrderBy( line => line.Timestamp )
			.Select( Text )
			.ToImmutableArray();
	}
}