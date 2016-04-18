using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	public class LoggerHistorySink : ILoggerHistory
	{
		readonly IList<LogEvent> source = new Collection<LogEvent>();
		readonly IReadOnlyCollection<LogEvent> events;

		public LoggerHistorySink()
		{
			events = new ReadOnlyCollection<LogEvent>( source );
		}

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events => events;

		public virtual void Emit( LogEvent logEvent )
		{
			source.Ensure( logEvent );
		}
	}

	public class LogEventMessageFactory : FactoryBase<IEnumerable<LogEvent>, string[]>
	{
		public static LogEventMessageFactory Instance { get; } = new LogEventMessageFactory();

		readonly MessageTemplateTextFormatter formatter;

		public LogEventMessageFactory( string template = "{Timestamp:HH:mm:ss:fff} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}" ) : this( new MessageTemplateTextFormatter( template, null ) ) {}

		public LogEventMessageFactory( MessageTemplateTextFormatter formatter )
		{
			this.formatter = formatter;
		}

		protected override string[] CreateItem( IEnumerable<LogEvent> parameter ) => parameter.OrderBy( line => line.Timestamp ).Select( Create ).ToArray();

		string Create( LogEvent logEvent )
		{
			var writer = new StringWriter();
			formatter.Format( logEvent, writer );
			var result = writer.ToString().Trim();
			return result;
		}
	}
}