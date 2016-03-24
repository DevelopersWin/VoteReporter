using DragonSpark.Activation.FactoryModel;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Model;
using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DragonSpark.Aspects;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public class TracerFactory : ConfiguringFactory<Tracer>
	{
		public TracerFactory( [Required] Action<string> output ) : this( new PurgeDiagnosticsCommand( output ), new List<TraceListener>() ) {}

		public TracerFactory( PurgeDiagnosticsCommand purgeCommand, IList<TraceListener> listeners ) 
			: this( new DiagnosticsFactory( listeners ).Create, new DisposeTracerCommand( purgeCommand, listeners ).Run, new ConfigureTracer( purgeCommand ).Run ) {}

		public TracerFactory( Func<IDiagnostics> diagnostics, Action<Tracer> dispose, Action<Tracer> configure )
			: base( new TracerFactoryCore( diagnostics, dispose ).Create, configure ) {}
	}

	public class ConfigureTracer : Command<Tracer>
	{
		readonly PurgeDiagnosticsCommand purge;

		public ConfigureTracer( [Required] PurgeDiagnosticsCommand purge )
		{
			this.purge = purge;
		}

		protected override void OnExecute( Tracer parameter )
		{
			purge.ExecuteWith( parameter.Diagnostics );
			parameter.Profiler.Start();
		}
	}

	public class DisposeTracerCommand : Command<Tracer>
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

		protected override void OnExecute( Tracer parameter )
		{
			purge.ExecuteWith( parameter.Diagnostics );
			listeners.Purge().Each( remove.Run );
		}
	}

	public class PurgeDiagnosticsCommand : Command<IDiagnostics>
	{
		readonly Action<string> output;

		public PurgeDiagnosticsCommand( [Required] Action<string> output )
		{
			this.output = output;
		}

		protected override void OnExecute( IDiagnostics parameter ) => parameter.Purge( output );
	}

	public class DiagnosticsFactory : DragonSpark.Diagnostics.DiagnosticsFactory
	{
		public DiagnosticsFactory( IList<TraceListener> listeners ) : this( new RecordingLogEventSink(), new LoggingLevelSwitch(), listeners ) {}

		public DiagnosticsFactory( RecordingLogEventSink sink, LoggingLevelSwitch levelSwitch, IList<TraceListener> listeners ) : this( new RecordingLoggerFactory( sink, levelSwitch ).Create, sink, levelSwitch, listeners ) {}

		public DiagnosticsFactory( Func<ILogger> source, RecordingLogEventSink sink, LoggingLevelSwitch levelSwitch, IList<TraceListener> listeners ) : base( new Factory( source, listeners ).Create, sink, levelSwitch ) {}
	}

	public class Factory : FactoryBase<ILogger>
	{
		readonly Func<ILogger> inner;
		readonly IList<TraceListener> listeners;

		public Factory( [Required] Func<ILogger> inner, [Required] IList<TraceListener> listeners )
		{
			this.inner = inner;
			this.listeners = listeners;
		}

		protected override ILogger CreateItem()
		{
			var command = new EnableTraceCommand( new LoggingTraceListenerFactory( listeners ).Create );
			var result = new TracingLoggerFactory( inner, command ).Create();
			return result;
		}
	}

	public class TracerFactoryCore : FactoryBase<Tracer>
	{
		readonly Func<IDiagnostics> diagnosticsSource;
		readonly Action<Tracer> dispose;

		public TracerFactoryCore( [Required] Func<IDiagnostics> diagnosticsSource, [Required] Action<Tracer> dispose )
		{
			this.diagnosticsSource = diagnosticsSource;
			this.dispose = dispose;
		}

		protected override Tracer CreateItem() => new Tracer( diagnosticsSource(), dispose );
	}

	// [Disposable( ThrowObjectDisposedException = true )]
	public class Tracer : IDisposable
	{
		readonly Action<Tracer> dispose;

		public Tracer( [Required] IDiagnostics diagnostics, Action<Tracer> dispose ) : this( diagnostics, new Profiler( diagnostics.Logger ), dispose ) {}

		public Tracer( [Required] IDiagnostics diagnostics, [Required] IProfiler profiler, [Required]Action<Tracer> dispose )
		{
			this.dispose = dispose;
			Diagnostics = diagnostics;
			Profiler = profiler;
		}

		[Child]
		public IDiagnostics Diagnostics { get; }

		[Child]
		public IProfiler Profiler { get; }

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose()
		{
			Diagnostics.Dispose();
			Profiler.Dispose();
			dispose( this );
		}
	}

	public class TracingLoggerFactory : ConfiguringFactory<ILogger>
	{
		public TracingLoggerFactory( Func<ILogger> inner, EnableTraceCommand enable ) : this( inner, enable.Run ) {}

		public TracingLoggerFactory( Func<ILogger> inner, Action<ILogger> configure ) : base( inner, configure ) {}
	}

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
