using Serilog;
using System;

namespace DragonSpark.Diagnostics
{
	public static class Defaults
	{
		public const string Template = "{Timestamp:HH:mm:ss:fff} [{Level}] ({$SourceContext}) {Message}{NewLine}{Exception}";

		public static Func<ILogger> Logger { get; } = DefaultLogger.Default.Get;
	}
}