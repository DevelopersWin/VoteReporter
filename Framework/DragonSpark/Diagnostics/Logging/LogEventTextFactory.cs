using System.IO;
using DragonSpark.Sources.Parameterized;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace DragonSpark.Diagnostics.Logging
{
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
}