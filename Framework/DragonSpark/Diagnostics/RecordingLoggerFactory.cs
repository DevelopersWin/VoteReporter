using System.Composition;
using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;

namespace DragonSpark.Diagnostics
{
	public class RecordingLoggerFactory : FactoryBase<ILogger>
	{
		public RecordingLoggerFactory() : this( new RecordingLogEventSink() ) {}

		public RecordingLoggerFactory( [Required]ILogEventSink sink )
		{
			Sink = sink;
		}

		[Export]
		public ILogEventSink Sink { get; }

		protected override ILogger CreateItem() => new LoggerConfiguration().WriteTo.Sink( Sink ).CreateLogger();
	}
}
