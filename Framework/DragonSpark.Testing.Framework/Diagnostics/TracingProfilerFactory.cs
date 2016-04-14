using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class TracingProfilerFactory : ProfilerFactory
	{
		public TracingProfilerFactory( [Required] Action<string> output, [CallerMemberName]string context = null ) : this( output, new LoggerHistorySink(), context ) {}

		public TracingProfilerFactory( [Required] Action<string> output, [Required] ILoggerHistory history, [CallerMemberName]string context = null ) 
			: this( new PurgeLoggerHistoryFixedCommand( history, output ), history, context ) {}

		TracingProfilerFactory( PurgeLoggerHistoryFixedCommand purgeCommand, ILoggerHistory history, string context ) 
			: base( new TracingLoggerFactory( history ).Create(), context, purgeCommand ) {}
	}

	public class SourceContextTransformer : TransformerBase<LoggerConfiguration>
	{
		public static SourceContextTransformer Instance { get; } = new SourceContextTransformer();

		protected override LoggerConfiguration CreateItem( LoggerConfiguration parameter ) => parameter.Enrich.FromLogContext();
	}

	public class TracingLoggerFactory : ConfiguringFactory<ILogger>
	{
		public TracingLoggerFactory( ILoggerHistory history ) : this( history, new LoggingLevelSwitch() ) {}

		TracingLoggerFactory( ILoggerHistory history, LoggingLevelSwitch levelSwitch ) 
			: base( 
				  new RecordingLoggerFactory( history, levelSwitch, SourceContextTransformer.Instance ).Create,
				  new LoggerTraceListenerTrackingCommand().Run
				  ) {}
	}

	public class LoggingTraceListenerFactory : FactoryBase<ILogger, TraceListener>
	{
		public static LoggingTraceListenerFactory Instance { get; } = new LoggingTraceListenerFactory();

		protected override TraceListener CreateItem( ILogger parameter ) => new SerilogTraceListener.SerilogTraceListener( parameter );
	}

	public class LoggerTraceListenerTrackingCommand : DisposingCommand<ILogger>
	{
		readonly Func<ILogger, TraceListener> factory;
		readonly IList<TraceListener> listeners;
		readonly AddItemCommand add;
		readonly RemoveItemCommand remove;

		public LoggerTraceListenerTrackingCommand() : this( LoggingTraceListenerFactory.Instance.Create, new List<TraceListener>(), new AddItemCommand( Trace.Listeners ), new RemoveItemCommand( Trace.Listeners ) ) {}

		public LoggerTraceListenerTrackingCommand( [Required] Func<ILogger, TraceListener> factory, [Required] IList<TraceListener> listeners, [Required] AddItemCommand add, [Required] RemoveItemCommand remove )
		{
			this.factory = factory;
			this.listeners = listeners;
			this.add = add;
			this.remove = remove;
		}

		protected override void OnExecute( ILogger parameter )
		{
			var listener = factory( parameter );
			add.ExecuteWith( listener );
			listeners.Add( listener );
		}

		protected override void OnDispose() => listeners.Purge().Each( remove.Run );
	}
}
