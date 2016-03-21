using System;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;

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

		public virtual RecordingLogEventSink Sink { get; }
		public virtual LoggingLevelSwitch LevelSwitch { get; }

		protected override ILogger CreateItem()
		{
			var result = new LoggerConfiguration()
				.WriteTo.Sink( Sink )
				.MinimumLevel.ControlledBy( LevelSwitch )
				.CreateLogger();
			new LoggingProperties.AssociatedSink( result ).Assign( Sink );
			new LoggingProperties.AssociatedSwitch( result ).Assign( LevelSwitch );
			return result;
		}
	}

	public static class LoggingProperties
	{
		public class AssociatedSink : AssociatedValue<ILogger, RecordingLogEventSink>
		{
			public AssociatedSink( ILogger instance ) : base( instance, typeof(AssociatedSink) ) {}
		}

		public class AssociatedSwitch : AssociatedValue<ILogger, Serilog.Core.LoggingLevelSwitch>
		{
			public AssociatedSwitch( ILogger instance ) : base( instance, typeof(AssociatedSwitch) ) {}
		}
	}
}
