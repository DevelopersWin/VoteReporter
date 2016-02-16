using System.Diagnostics;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public class MessageLogger : MessageLoggerBase
	{
		public static ILogger Create() => new LoggerConfiguration().CreateLogger(); // TODO: ILogger Instance

		public static MessageLogger Instance { get; } = new MessageLogger();

		protected override void OnLog( Message message ) => Debug.WriteLine( message.Text );
	}
}
