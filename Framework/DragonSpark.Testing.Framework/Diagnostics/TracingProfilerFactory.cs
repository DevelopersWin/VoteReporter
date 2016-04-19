using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class ProfilerFactory<T> : ConfiguringFactory<MethodBase, IProfiler> where T : Category.Factory
	{
		readonly IDisposable[] disposables;

		// public ProfilerFactory( Action<string> output ) : this( output, new LoggerHistorySink() ) {}

		// public ProfilerFactory( Action<string> output, ILoggerHistory history ) : this( output, history, LoggingLevelSwitchFactory.Instance.Create(), new List<TraceListener>() ) {}

		// public ProfilerFactory( Action<string> output, ILoggerHistory history, LoggingLevelSwitch levelSwitch, IList<TraceListener> listeners ) : this( output, new RecordingLoggerFactory( history, levelSwitch ).Create(), history, listeners ) {}

		public ProfilerFactory( Action<string> output, ILogger logger, ILoggerHistory history ) : this( output, logger, history, new List<TraceListener>() ) {}

		public ProfilerFactory( Action<string> output, ILogger logger, ILoggerHistory history, IList<TraceListener> listeners ) : this( output, logger, new PurgeLoggerMessageHistoryCommand( history ), new LoggerTraceListenerTrackingCommand( listeners ) ) {}

		ProfilerFactory( Action<string> output, ILogger logger, PurgeLoggerMessageHistoryCommand purge, LoggerTraceListenerTrackingCommand tracker ) : this( logger, tracker, () => purge.ExecuteWith( output ) ) {}
		
		ProfilerFactory( ILogger logger, LoggerTraceListenerTrackingCommand command, Action purge ) : this( new ConfiguringFactory<MethodBase, ILogger>( new MethodLoggerFactory( logger ).Create, command.Run ).Create, new CompositeCommand( new DelegatedCommand( purge ), StartProcessCommand.Instance ), command, new DisposableAction( purge ) ) {}

		ProfilerFactory( Func<MethodBase, ILogger> loggerSource, ICommand<IProfiler> command, params IDisposable[] disposables ) : base( new Windows.Diagnostics.ProfilerFactory<T>( loggerSource ).Create, command.Run )
		{
			this.disposables = disposables;
		}

		protected override IProfiler CreateItem( MethodBase parameter ) => base.CreateItem( parameter ).AssociateForDispose( disposables );
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

		// public LoggerTraceListenerTrackingCommand() : this( new List<TraceListener>() ) {}

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
