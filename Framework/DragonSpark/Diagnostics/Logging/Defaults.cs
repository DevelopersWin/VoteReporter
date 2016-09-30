using System;
using DragonSpark.Sources;
using Serilog;

namespace DragonSpark.Diagnostics.Logging
{
	public static class Defaults
	{
		public static ISource<ILogger> Source { get; } = Logger.Default.ToScope();

		public static Func<ILogger> Factory { get; } = Source.Get;
	}
}