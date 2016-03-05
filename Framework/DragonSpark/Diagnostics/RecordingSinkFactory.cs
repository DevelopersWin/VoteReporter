using System.Composition;
using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;

namespace DragonSpark.Diagnostics
{
	public class RecordingSinkFactory : FactoryBase<ILogger>
	{
		public RecordingSinkFactory() : this( new RecordingLogEventSink() ) {}

		public RecordingSinkFactory( [Required]ILogEventSink sink )
		{
			Sink = sink;
		}

		[Export]
		public ILogEventSink Sink { get; }

		protected override ILogger CreateItem() => new LoggerConfiguration().WriteTo.Sink( Sink ).CreateLogger();
	}
}
