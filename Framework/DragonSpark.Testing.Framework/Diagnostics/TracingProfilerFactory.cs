using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class ProfilerFactory<T> : Windows.Diagnostics.ProfilerFactory<T> where T : Category.Factory
	{
		readonly Action<string> output;
		readonly ILoggerHistory history;
		readonly IDisposable tracker;

		public ProfilerFactory( [Required] Action<string> output ) : this( output, new LoggerHistorySink() ) {}

		public ProfilerFactory( [Required] Action<string> output, ILoggerHistory history ) : this( output, history, new List<TraceListener>() ) {}

		public ProfilerFactory( [Required] Action<string> output, ILoggerHistory history, IList<TraceListener> listeners ) : this( output, history, listeners, new LoggingLevelSwitch() ) {}

		public ProfilerFactory( [Required] Action<string> output, ILoggerHistory history, IList<TraceListener> listeners, LoggingLevelSwitch levelSwitch ) : this( output, history, new RecordingLoggerFactory( history, levelSwitch ).Create(), listeners ) {}

		public ProfilerFactory( [Required] Action<string> output, ILoggerHistory history, ILogger logger ) : this( output, history, logger, new List<TraceListener>() ) {}

		public ProfilerFactory( [Required] Action<string> output, ILoggerHistory history, ILogger logger, IList<TraceListener> listeners ) : this( output, history, logger, new LoggerTraceListenerTrackingCommand( listeners) ) {}

		public ProfilerFactory( [Required] Action<string> output, ILoggerHistory history, ILogger logger, LoggerTraceListenerTrackingCommand command ) : this( output, history, command, new ConfiguringFactory<MethodBase, ILogger>( new MethodLoggerFactory( logger ).Create, command.Run ).Create ) {}

		public ProfilerFactory( [Required] Action<string> output, ILoggerHistory history, IDisposable tracker, Func<MethodBase, ILogger> loggerSource ) : base( loggerSource )
		{
			this.output = output;
			this.history = history;
			this.tracker = tracker;
		}

		protected override IProfiler CreateItem( MethodBase parameter )
		{
			Action purge = () => new PurgeLoggerMessageHistoryCommand( history ).ExecuteWith( output );
			purge();

			var result = base.CreateItem( parameter ).AssociateForDispose( tracker, new DisposableAction( purge ) ).With( StartProcessCommand.Instance.Run );
			return result;
		}
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
