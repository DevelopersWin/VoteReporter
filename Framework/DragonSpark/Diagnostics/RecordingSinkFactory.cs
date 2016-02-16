using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;

namespace DragonSpark.Diagnostics
{
	public class RecordingSinkFactory : FactoryBase<ILogger>
	{
		// public static RecordingSinkFactory Instance { get; } = new RecordingSinkFactory();

		readonly ILogEventSink sink;

		public RecordingSinkFactory() : this( new RecordingLogEventSink() ) {}

		public RecordingSinkFactory( [Required]ILogEventSink sink )
		{
			this.sink = sink;
		}

		protected override ILogger CreateItem() => new LoggerConfiguration().WriteTo.Sink( sink ).CreateLogger();
	}
}
