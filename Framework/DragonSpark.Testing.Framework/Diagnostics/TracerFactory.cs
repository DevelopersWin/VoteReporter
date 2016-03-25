using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class TracerFactory : ConfiguringFactory<ITracer>
	{
		public TracerFactory( [Required] Action<string> output ) : this( output, new LoggerHistorySink() ) {}

		public TracerFactory( [Required] Action<string> output, [Required] ILoggerHistory history, [CallerMemberName]string context = null ) 
			: this( new PurgeDiagnosticsCommand( history, output ), history, new List<TraceListener>(), context ) {}

		TracerFactory( PurgeDiagnosticsCommand purgeCommand, ILoggerHistory history, IList<TraceListener> listeners, string context ) 
			: this( new DiagnosticsFactory( history, listeners ).Create, context, new DisposeTracerCommand( purgeCommand, listeners ).Run, new ConfigureTracer( purgeCommand ).Run ) {}

		public TracerFactory( Func<IDiagnostics> diagnostics, string context, Action<ITracer> dispose, Action<ITracer> configure )
			: base( () => new Tracer( diagnostics(), context, dispose ), configure ) {}
	}

	public class ConfigureTracer : Command<ITracer>
	{
		readonly PurgeDiagnosticsCommand purge;

		public ConfigureTracer( [Required] PurgeDiagnosticsCommand purge )
		{
			this.purge = purge;
		}

		protected override void OnExecute( ITracer parameter )
		{
			purge.ExecuteWith( parameter );
			parameter.Start();
		}
	}

	public class DisposeTracerCommand : Command<ITracer>
	{
		readonly PurgeDiagnosticsCommand purge;
		readonly IList<TraceListener> listeners;
		readonly RemoveItemCommand remove;

		public DisposeTracerCommand( [Required] PurgeDiagnosticsCommand purge, IList<TraceListener> listeners ) : this( purge, listeners, new RemoveItemCommand( Trace.Listeners ) ) {}

		public DisposeTracerCommand( [Required] PurgeDiagnosticsCommand purge, IList<TraceListener> listeners, RemoveItemCommand remove )
		{
			this.purge = purge;
			this.listeners = listeners;
			this.remove = remove;
		}

		protected override void OnExecute( ITracer parameter )
		{
			purge.ExecuteWith( parameter );
			listeners.Purge().Each( remove.Run );
		}
	}

	public class PurgeDiagnosticsCommand : FixedCommand
	{
		public PurgeDiagnosticsCommand( [Required] ILoggerHistory history, [Required] Action<string> output ) : this( new PurgeLoggerMessageHistoryCommand( history ), output ) {}
		public PurgeDiagnosticsCommand( [Required] PurgeLoggerMessageHistoryCommand command, [Required] Action<string> output ) : base( command, output ) {}
	}

	public class DiagnosticsFactory : DragonSpark.Diagnostics.DiagnosticsFactory
	{
		public DiagnosticsFactory( [Required] ILoggerHistory history, [Required] IList<TraceListener> listeners ) : this( history, new LoggingLevelSwitch(), listeners ) {}

		public DiagnosticsFactory( ILoggerHistory sink, LoggingLevelSwitch levelSwitch, IList<TraceListener> listeners ) : base( new TracingLoggerFactory( sink, levelSwitch, listeners ).Create, levelSwitch ) {}
	}

	public class RecordingLoggerFactory : DragonSpark.Diagnostics.RecordingLoggerFactory
	{
		public RecordingLoggerFactory( ILoggerHistory history, LoggingLevelSwitch levelSwitch ) 
			: base( history, levelSwitch, new RecordingLoggingConfigurationFactory( history, levelSwitch, SourceContextTransformer.Instance ).Create ) {}
	}

	public class SourceContextTransformer : TransformerBase<LoggerConfiguration>
	{
		public static SourceContextTransformer Instance { get; } = new SourceContextTransformer();

		protected override LoggerConfiguration CreateItem( LoggerConfiguration parameter ) => parameter.Enrich.FromLogContext();
	}

	public class TracingLoggerFactory : FactoryBase<ILogger>
	{
		readonly Func<ILogger> inner;
		readonly IList<TraceListener> listeners;

		public TracingLoggerFactory( ILoggerHistory sink, LoggingLevelSwitch levelSwitch, [Required] IList<TraceListener> listeners ) 
			: this( new RecordingLoggerFactory( sink, levelSwitch ).Create, listeners ) {}

		TracingLoggerFactory( [Required] Func<ILogger> inner, [Required] IList<TraceListener> listeners )
		{
			this.inner = inner;
			this.listeners = listeners;
		}

		protected override ILogger CreateItem()
		{
			var command = new EnableTraceCommand( new LoggingTraceListenerFactory( listeners ).Create );
			var result = new ConfiguringFactory<ILogger>( inner, command.Run ).Create();
			return result;
		}
	}

	/*public class TracerFactoryCore : FactoryBase<Tracer>
	{
		readonly Func<IDiagnostics> diagnosticsSource;
		readonly Action<ITracer> dispose;

		public TracerFactoryCore( [Required] Func<IDiagnostics> diagnosticsSource, [Required] Action<ITracer> dispose )
		{
			this.diagnosticsSource = diagnosticsSource;
			this.dispose = dispose;
		}

		protected override Tracer CreateItem() => new Tracer( diagnosticsSource(), dispose );
	}*/

	public interface ITracer : IDiagnostics, IProfiler {}

	// [Disposable( ThrowObjectDisposedException = true )]
	public class Tracer : ITracer
	{
		readonly IDiagnostics diagnostics;
		readonly IProfiler profiler;
		readonly Action<Tracer> dispose;

		public Tracer( [Required] IDiagnostics diagnostics, string context, Action<ITracer> dispose ) : this( diagnostics, new Profiler( diagnostics.Logger, context ), dispose ) {}

		public Tracer( [Required] IDiagnostics diagnostics, [Required] IProfiler profiler, [Required]Action<ITracer> dispose )
		{
			this.diagnostics = diagnostics;
			this.profiler = profiler;
			this.dispose = dispose;
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose()
		{
			profiler.Dispose();
			dispose( this );
		}

		public ILogger Logger => diagnostics.Logger;

		public LoggingLevelSwitch Switch => diagnostics.Switch;

		public void Mark( string @event ) => profiler.Mark( @event );

		public void Start() => profiler.Start();
	}

	/*public class TracingLoggerFactory : ConfiguringFactory<ILogger>
	{
		//public TracingLoggerFactory( Func<ILogger> inner, EnableTraceCommand enable ) : this( inner, enable.Run ) {}

		public TracingLoggerFactory( Func<ILogger> inner, Action<ILogger> configure ) : base( inner, configure ) {}
	}*/

	public class AddItemCommand<T> : Command<T>
	{
		readonly IList<T> list;

		public AddItemCommand( [Required] IList<T> list )
		{
			this.list = list;
		}

		protected override void OnExecute( T parameter ) => list.Add( parameter );
	}

	public class AddItemCommand : Command<object>
	{
		readonly IList list;

		public AddItemCommand( [Required] IList list )
		{
			this.list = list;
		}

		protected override void OnExecute( object parameter ) => list.Add( parameter );
	}

	public class RemoveItemCommand : Command<object>
	{
		readonly IList list;

		public RemoveItemCommand( [Required] IList list )
		{
			this.list = list;
		}

		protected override void OnExecute( object parameter ) => list.Remove( parameter );
	}

	public class LoggingTraceListenerFactory : ConfiguringFactory<ILogger, TraceListener>
	{
		public LoggingTraceListenerFactory( [Required] IList<TraceListener> listeners ) 
			: base( logger => new SerilogTraceListener.SerilogTraceListener( logger ), new AddItemCommand<TraceListener>( listeners ).Run ) {}
	}

	public class EnableTraceCommand : Command<ILogger>
	{
		readonly Func<ILogger, TraceListener> factory;
		readonly AddItemCommand add;
		
		public EnableTraceCommand( Func<ILogger, TraceListener> factory ) : this( factory, new AddItemCommand( Trace.Listeners ) ) {}

		public EnableTraceCommand( [Required] Func<ILogger, TraceListener> factory, [Required] AddItemCommand add )
		{
			this.factory = factory;
			this.add = add;
		}

		protected override void OnExecute( ILogger parameter ) => add.ExecuteWith( factory( parameter ) );
	}
}
