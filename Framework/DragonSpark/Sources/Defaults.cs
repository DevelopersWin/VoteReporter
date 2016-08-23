using System;
using DragonSpark.Application;

namespace DragonSpark.Sources
{
	public static class Defaults
	{
		public static string TimerTemplate { get; } = "Executing Delegate {@Method}.";
		public static string ParameterizedTimerTemplate { get; } = "Executing Delegate {@Method} with parameter {Parameter}.";
		public static Func<IExportProvider> DefaultSource { get; } = Exports.Default.Get;
	}
}