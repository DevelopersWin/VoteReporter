using Serilog;

namespace DragonSpark.Diagnostics
{
	public class MessageLogger
	{
		public static ILogger Create() => new LoggerConfiguration().CreateLogger(); // TODO: ILogger Instance
	}
}
