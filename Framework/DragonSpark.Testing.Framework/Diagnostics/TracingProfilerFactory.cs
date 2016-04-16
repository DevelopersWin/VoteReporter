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

namespace DragonSpark.Testing.Framework.Diagnostics
{
	/*public class TracingProfilerFactory<T> : ProfilerFactory<T> where T : Category.Factory
	{
		// public TracingProfilerFactory( [Required] Action<string> output, [CallerMemberName]string context = null ) : this( output, new LoggerHistorySink(), context ) {}

		public TracingProfilerFactory( [Required] Action<string> output, [Required] ILoggerHistory history, [CallerMemberName]string context = null ) 
			: this( new PurgeLoggerHistoryFixedCommand( history, output ), history, context ) {}

		TracingProfilerFactory( PurgeLoggerHistoryFixedCommand purgeCommand, ILoggerHistory history, string context ) 
			: base( new TracingLoggerFactory( history ).Create(), context, purgeCommand ) {}
	}*/

	public class TracingLoggerFactory : ConfiguringFactory<ILogger>
	{
		public TracingLoggerFactory( ILoggerHistory history ) : this( history, new LoggingLevelSwitch(), new LoggerTraceListenerTrackingCommand() ) {}

		//public TracingLoggerFactory( ILoggerHistory history, LoggerTraceListenerTrackingCommand command ) : this( history, new LoggingLevelSwitch(), command ) {}

		public TracingLoggerFactory( ILoggerHistory history, LoggingLevelSwitch levelSwitch, ICommand<ILogger> command ) 
			: base( new RecordingLoggerFactory( history, levelSwitch ).Create, command.Run ) {}
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

		public LoggerTraceListenerTrackingCommand() : this( new List<TraceListener>() ) {}

		public LoggerTraceListenerTrackingCommand( IList<TraceListener> listeners ) : this( LoggingTraceListenerFactory.Instance.Create, listeners, new AddItemCommand( Trace.Listeners ), new RemoveItemCommand( Trace.Listeners ) ) {}

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
