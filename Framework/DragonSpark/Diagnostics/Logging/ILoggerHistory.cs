using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging
{
	public interface ILoggerHistory : ILogEventSink
	{
		IEnumerable<LogEvent> Events { get; }

		void Clear();
	}
}