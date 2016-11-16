using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public sealed class DefaultSystemLoggerConfigurations : ItemSource<IAlteration<LoggerConfiguration>>
	{
		public static DefaultSystemLoggerConfigurations Default { get; } = new DefaultSystemLoggerConfigurations();
		DefaultSystemLoggerConfigurations() : base( HistoryAlteration.Implementation.Append( DefaultLoggerConfigurations.Default ) ) {}

		sealed class HistoryAlteration : AlterationBase<LoggerConfiguration>
		{
			public static HistoryAlteration Implementation { get; } = new HistoryAlteration();
			HistoryAlteration() : this( LoggingHistory.Default ) {}

			readonly ILoggerHistory history;

			HistoryAlteration( ILoggerHistory history )
			{
				this.history = history;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history );
		}
	}
}