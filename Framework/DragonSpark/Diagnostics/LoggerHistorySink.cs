using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;

namespace DragonSpark.Diagnostics
{
	[ThreadUnsafe]
	public class LoggerHistorySink : ILoggerHistory
	{
		[Reference]
		readonly IList<LogEvent> source = new Collection<LogEvent>();

		[Reference]
		readonly IReadOnlyCollection<LogEvent> events;

		int threadId;
		int? taskId;

		public LoggerHistorySink()
		{
			threadId = Environment.CurrentManagedThreadId;
			taskId = Task.CurrentId;

			events = new ReadOnlyCollection<LogEvent>( source );
		}

		public void Clear() => source.Clear();

		public IEnumerable<LogEvent> Events
		{
			get
			{
				/*if ( Environment.CurrentManagedThreadId != threadId )
				{
					throw new InvalidOperationException( $"WTF! {taskId} - {Task.CurrentId} - {SynchronizationContext.Current}" );
				}*/

				return events;
			}
		}

		public virtual void Emit( [Required]LogEvent logEvent )
		{
			/*if ( Environment.CurrentManagedThreadId != threadId )
			{
				throw new InvalidOperationException( $"WTF! {taskId} - {Task.CurrentId}" );
			}*/

			if ( logEvent == null )
			{
				throw new InvalidOperationException( "WTF!" );
			}
			source.Ensure( logEvent );

			if ( source.Any( @e => e == null ) )
			{
				throw new InvalidOperationException( "WTF!" );
			}
		}
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