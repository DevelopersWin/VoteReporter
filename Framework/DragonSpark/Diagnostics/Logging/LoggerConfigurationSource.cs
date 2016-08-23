using System;
using DragonSpark.Sources.Parameterized;
using Serilog;

namespace DragonSpark.Diagnostics.Logging
{
	sealed class LoggerConfigurationSource : LoggerConfigurationSourceBase
	{
		public static LoggerConfigurationSource Default { get; } = new LoggerConfigurationSource();
		LoggerConfigurationSource() : base( HistoryTransform.DefaultNested ) {}

		sealed class HistoryTransform : TransformerBase<LoggerConfiguration>
		{
			public static HistoryTransform DefaultNested { get; } = new HistoryTransform();
			HistoryTransform() : this( LoggingHistory.Default.Get ) {}

			readonly Func<ILoggerHistory> history;

			HistoryTransform( Func<ILoggerHistory> history )
			{
				this.history = history;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history() );
		}
	}
}