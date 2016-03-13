using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public class RecordingLoggerFactory : FactoryBase<ILogger>
	{
		public RecordingLoggerFactory() : this( new RecordingLogEventSink(), new LoggingLevelSwitch() ) {}

		public RecordingLoggerFactory( [Required]RecordingLogEventSink sink, [Required]Serilog.Core.LoggingLevelSwitch levelSwitch )
		{
			Sink = sink;
			LevelSwitch = levelSwitch;
		}

		public RecordingLogEventSink Sink { get; }
		public Serilog.Core.LoggingLevelSwitch LevelSwitch { get; }

		protected override ILogger CreateItem() => new LoggerConfiguration()
			.WriteTo.Sink( Sink )
			.MinimumLevel.ControlledBy( LevelSwitch )
			.CreateLogger();
	}
}
