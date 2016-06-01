using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Threading;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using PostSharp.Patterns.Model;

namespace DragonSpark.Diagnostics
{
	[ReaderWriterSynchronized]
	public class LoggerHistorySink : ILoggerHistory
	{
		[Reference]
		readonly IList<LogEvent> source = new Collection<LogEvent>();

		[Writer]
		public void Clear() => source.Clear();

		public ImmutableArray<LogEvent> Events => source.ToImmutableArray();

		[Writer]
		public virtual void Emit( [Required]LogEvent logEvent ) => source.Ensure( logEvent );
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
			.OrderBy( line => line.Timestamp )
			.Select( LogEventTextFactory.Instance.Create )
			.ToArray();
	}
}