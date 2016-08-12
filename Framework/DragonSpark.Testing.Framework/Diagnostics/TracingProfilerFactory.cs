using DragonSpark.Runtime;
using Serilog;
using System;
using System.Diagnostics;
using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	/*public class ProfilerFactory : Windows.Diagnostics.ProfilerFactory
	{
		readonly Action<string> output;
		readonly ILoggerHistory history;

		public ProfilerFactory( Action<string> output ) : this( output, GlobalServiceProvider.GetService<ILoggerHistory>() ) {}

		public ProfilerFactory( Action<string> output, ILoggerHistory history ) : this( output, history, DiagnosticProperties.Logger.Get ) {}
		public ProfilerFactory( Action<string> output, ILoggerHistory history, Func<MethodBase, ILogger> loggerSource ) : base( loggerSource )
		{
			this.output = output;
			this.history = history;
		}

		public override IProfiler Create( MethodBase parameter )
		{
			var purge = new FixedCommand<Action<string>>( new PurgeLoggerMessageHistoryCommand( history ), output );
			var start = new CompositeCommand( purge, StartProcessCommand.Instance );
			var result = base.Create( parameter ).With( start.Execute ).AssociateForDispose( new DisposableAction( purge.Run ) );
			return result;
		}
	}*/

	public static class Tracing
	{
		// public static IAttachedProperty<ILogger, IList<TraceListener>> Listeners { get; } = new AttachedProperty<ILogger, IList<TraceListener>>( logger => new List<TraceListener>() );

		public static ICache<ILogger, TraceListener> Listener { get; } = new Cache<ILogger, TraceListener>( logger => new SerilogTraceListener.SerilogTraceListener() );

		public static IDisposable WithTracing( this ILogger @this ) => new TracingAssignment( Listener.Get( @this ) );
	}

	class TracingAssignment : Assignment<TraceListener, CollectionAction>
	{
		public TracingAssignment( TraceListener first ) : base( new CollectionAssign<TraceListener>( Trace.Listeners ), Assignments.From( first ), new Value<CollectionAction>( CollectionAction.Add, CollectionAction.Remove ) ) {}
		// public TracingAssignment( IAssign<TraceListener, CollectionAction> assign, Value<TraceListener> first, Value<CollectionAction> second ) : base( assign, first, second ) {}
	}

	/*public class TraceAwareProfilerFactory : FactoryBase<ILogger, IDisposable>
	{
		public override IDisposable Create( ILogger parameter )
		{
			var result = new LoggerTraceListenerTrackingCommand(  );
			parameter.With( result.Run );
			return result;
		}
	}*/
	
	/*public class LoggingTraceListenerFactory : FactoryBase<ILogger, TraceListener>
	{
		public static LoggingTraceListenerFactory Instance { get; } = new LoggingTraceListenerFactory();

		public override TraceListener Create( ILogger parameter ) => new SerilogTraceListener.SerilogTraceListener( parameter );
	}*/

	
	/*public class LoggerTraceListenerTrackingCommand : DisposingCommand<ILogger>
	{
		readonly Func<ILogger, TraceListener> factory;
		readonly Action<TraceListener> add;
		readonly Action<TraceListener> remove;

		public LoggerTraceListenerTrackingCommand() : this( Defaults.Factory, Defaults.Add, Defaults.Remove ) {}

		public LoggerTraceListenerTrackingCommand( Func<ILogger, TraceListener> factory, Action<TraceListener> add, Action<TraceListener> remove )
		{
			this.factory = factory;
			this.add = add;
			this.remove = remove;
		}

		public override void Execute( ILogger parameter )
		{
			var listener = factory( parameter );
			add( listener );
			Properties.Listeners.Get( parameter ).Add( listener );
		}

		protected override void OnDispose() => listeners.Purge().Each( remove );

		static class Defaults
		{
			public static Func<ILogger, TraceListener> Factory { get; } = LoggingTraceListenerFactory.Instance.Create;
			public static Action<TraceListener> Add { get; } = new AddItemCommand( Trace.Listeners ).Execute;
			public static Action<TraceListener> Remove { get; } = new RemoveItemCommand( Trace.Listeners ).Execute;
		}
	}*/
}
