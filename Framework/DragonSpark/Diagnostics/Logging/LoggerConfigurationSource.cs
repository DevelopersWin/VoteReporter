using DragonSpark.Sources.Parameterized;
using Serilog;
using System;

namespace DragonSpark.Diagnostics.Logging
{
	sealed class LoggerConfigurationSource : LoggerConfigurationSourceBase
	{
		public static LoggerConfigurationSource Default { get; } = new LoggerConfigurationSource();
		LoggerConfigurationSource() : base( HistoryAlteration.DefaultNested ) {}

		sealed class HistoryAlteration : AlterationBase<LoggerConfiguration>
		{
			public static HistoryAlteration DefaultNested { get; } = new HistoryAlteration();
			HistoryAlteration() : this( LoggingHistory.Default.Get ) {}

			readonly Func<ILoggerHistory> history;

			HistoryAlteration( Func<ILoggerHistory> history )
			{
				this.history = history;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history() );
		}
	}
}