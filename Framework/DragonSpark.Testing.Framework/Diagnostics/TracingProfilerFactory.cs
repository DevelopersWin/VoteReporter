using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class TraceAwareProfilerFactory : FactoryBase<MethodBase, IProfiler>
	{
		readonly Action<string> output;
		readonly ILogger logger;
		readonly ILoggerHistory history;
		readonly IList<TraceListener> listeners;

		public TraceAwareProfilerFactory( Action<string> output, ILogger logger, ILoggerHistory history ) : this( output, logger, history, new List<TraceListener>() ) {}

		public TraceAwareProfilerFactory( Action<string> output, ILogger logger, ILoggerHistory history, IList<TraceListener> listeners )
		{
			this.output = output;
			this.logger = logger;
			this.history = history;
			this.listeners = listeners;
		}

		public override IProfiler Create( MethodBase parameter )
		{
			var command = new PurgeLoggerMessageHistoryCommand( history );
			var tracker = new LoggerTraceListenerTrackingCommand( listeners );
			var configured = new ConfiguringFactory<MethodBase, ILogger>( new LoggerFromMethodFactory( logger.Self ).Create, tracker.Run );
			var inner = new Windows.Diagnostics.ProfilerFactory( configured.Create );
			var purge = new FixedCommand( command, output );
			var start = new CompositeCommand( purge, StartProcessCommand.Instance );
			var result = inner.Create( parameter ).With( start.Run ).AssociateForDispose( new DisposableAction( purge.Run ), tracker );
			return result;
		}
	}
	
	public class LoggingTraceListenerFactory : FactoryBase<ILogger, TraceListener>
	{
		public static LoggingTraceListenerFactory Instance { get; } = new LoggingTraceListenerFactory();

		public override TraceListener Create( ILogger parameter ) => new SerilogTraceListener.SerilogTraceListener( parameter );
	}

	public class LoggerTraceListenerTrackingCommand : DisposingCommand<ILogger>
	{
		readonly Func<ILogger, TraceListener> factory;
		readonly IList<TraceListener> listeners;
		readonly AddItemCommand add;
		readonly RemoveItemCommand remove;

		public LoggerTraceListenerTrackingCommand( IList<TraceListener> listeners ) : this( Defaults.Factory, listeners, Defaults.AddItemCommand, Defaults.RemoveItemCommand ) {}

		public LoggerTraceListenerTrackingCommand( Func<ILogger, TraceListener> factory, IList<TraceListener> listeners, AddItemCommand add, RemoveItemCommand remove )
		{
			this.factory = factory;
			this.listeners = listeners;
			this.add = add;
			this.remove = remove;
		}

		public override void Execute( ILogger parameter )
		{
			var listener = factory( parameter );
			add.Run( listener );
			listeners.Add( listener );
		}

		protected override void OnDispose() => listeners.Purge().Each( remove.Run );

		static class Defaults
		{
			public static Func<ILogger, TraceListener> Factory { get; } = LoggingTraceListenerFactory.Instance.Create;
			public static AddItemCommand AddItemCommand { get; } = new AddItemCommand( Trace.Listeners );
			public static RemoveItemCommand RemoveItemCommand { get; } = new RemoveItemCommand( Trace.Listeners );
		}
	}
}
