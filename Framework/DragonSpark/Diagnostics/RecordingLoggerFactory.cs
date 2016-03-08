using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public class RecordingLoggerFactory : FactoryBase<ILogger>
	{
		readonly RecordingLogEventSink sink;

		public RecordingLoggerFactory() : this( new RecordingLogEventSink() ) {}

		public RecordingLoggerFactory( [Required]RecordingLogEventSink sink )
		{
			this.sink = sink;
		}

		protected override ILogger CreateItem() => new LoggerConfiguration().WriteTo.Sink( sink ).CreateLogger();
	}
}
