using DragonSpark.Activation;
using PostSharp.Patterns.Contracts;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	public class LoggerHistorySink : ILoggerHistory
	{
		readonly ConcurrentStack<LogEvent> source = new ConcurrentStack<LogEvent>();

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events => source.ToArray().Reverse().ToArray();

		public virtual void Emit( [Required]LogEvent logEvent ) => source.Push( logEvent );
	}

	public class LogEventTextFactory : FactoryBase<LogEvent, string>
	{
		public static LogEventTextFactory Instance { get; } = new LogEventTextFactory();

		readonly MessageTemplateTextFormatter formatter;

		public LogEventTextFactory( string template = "{Timestamp:HH:mm:ss:fff} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}" ) : this( new MessageTemplateTextFormatter( template, null ) ) {}

		public LogEventTextFactory( MessageTemplateTextFormatter formatter )
		{
			this.formatter = formatter;
		}

		public override string Create( LogEvent parameter )
		{
			var writer = new StringWriter();
			formatter.Format( parameter, writer );
			var result = writer.ToString().Trim();
			return result;
		}
	}

	public class LogEventMessageFactory : FactoryBase<IEnumerable<LogEvent>, string[]>
	{
		public static LogEventMessageFactory Instance { get; } = new LogEventMessageFactory();

		public override string[] Create( IEnumerable<LogEvent> parameter ) => parameter
			// .OrderBy( line => line.Timestamp )
			.Select( LogEventTextFactory.Instance.ToDelegate() )
			.ToArray();
	}
}