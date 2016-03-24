using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace DragonSpark.Diagnostics
{
	public class DiagnosticsFactory : FactoryBase<IDiagnostics>
	{
		readonly Func<ILogger> source;
		readonly RecordingLogEventSink sink;
		readonly LoggingLevelSwitch levelSwitch;
		public DiagnosticsFactory() : this( new RecordingLogEventSink(), new LoggingLevelSwitch() ) {}

		DiagnosticsFactory( [Required]RecordingLogEventSink sink, [Required]LoggingLevelSwitch levelSwitch ) : this( new RecordingLoggerFactory( sink, levelSwitch ).Create, sink, levelSwitch )  {}

		public DiagnosticsFactory( [Required] Func<ILogger> source, [Required]RecordingLogEventSink sink, [Required]LoggingLevelSwitch levelSwitch )
		{
			this.source = source;
			this.sink = sink;
			this.levelSwitch = levelSwitch;
		}

		protected override IDiagnostics CreateItem()
		{
			var logger = source();
			var result = new Diagnostics( logger, sink, levelSwitch );
			return result;
		}
	}

	/*public class ProfilerFactory : FactoryBase<IProfiler>
	{
		public ProfilerFactory() : this( new RecordingLoggerFactory() ) {}

		ProfilerFactory( RecordingLoggerFactory factory ) : this( new DiagnosticsFactory( factory.Create, factory.Sink, factory.LevelSwitch ).Create, factory.Sink ) {}

		public ProfilerFactory( [Required] Func<IDiagnostics> source, [Required] RecordingLogEventSink sink )
		{
			Source = source;
			Sink = sink;
		}

		protected Func<IDiagnostics> Source { get; }
		protected RecordingLogEventSink Sink { get; }

		protected override IProfiler CreateItem() => new Profiler( Source(), Sink );
	}*/

	public interface IDiagnostics : IDisposable
	{
		ILogger Logger { get; }

		LoggingLevelSwitch Switch { get; }

		IEnumerable<LogEvent> Events { get; }

		void Purge( Action<string> writer );
	}

	public static class DiagnosticsExtensions
	{
		public static void Complete( [Required] this IDiagnostics @this, [Required] Action<string> writer )
		{
			@this.Purge( writer );
			@this.Dispose();
		}
	}

	/*public class AssignTracingContext<T> : AssignValueCommand<T>
	{
		readonly Action<string> output;

		public AssignTracingContext( [Required] IDiagnostics diagnostics, [Required] Action<string> output, [Required] IWritableValue<T> context ) : base( context )
		{
			Diagnostics = diagnostics;
			this.output = output;
		}

		public IDiagnostics Diagnostics { get; }

		protected override void OnExecute( T parameter )
		{
			Diagnostics.Purge( output );
			base.OnExecute( parameter );
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			Diagnostics.Complete( output );
		}
	}*/

	public class RecordingLoggingConfigurationFactory : AggregateFactory<LoggerConfiguration>
	{
		public RecordingLoggingConfigurationFactory() : this( new RecordingLogEventSink(), new LoggingLevelSwitch() ) {}

		public RecordingLoggingConfigurationFactory( [Required] RecordingLogEventSink sink, [Required] LoggingLevelSwitch controller ) 
			: base( new LoggingConfigurationCoreFactory( controller ), new RecordingLoggingConfigurationTransformer( sink ) ) {}
	}

	public class LoggingFactory : FactoryBase<ILogger>
	{
		readonly Func<LoggerConfiguration> source;

		public LoggingFactory( [Required] Func<LoggerConfiguration> source )
		{
			this.source = source;
		}

		protected override ILogger CreateItem() => source().CreateLogger();
	}

	public class LoggingConfigurationCoreFactory : FactoryBase<LoggerConfiguration>
	{
		readonly LoggerConfiguration configuration;
		readonly LoggingLevelSwitch controller;

		// public LoggingConfigurationCoreFactory() : this( new LoggerConfiguration(), new LoggingLevelSwitch() ) {}

		public LoggingConfigurationCoreFactory( [Required] LoggingLevelSwitch logging ) : this( new LoggerConfiguration(), logging ) {}

		public LoggingConfigurationCoreFactory( [Required] LoggerConfiguration configuration, [Required] LoggingLevelSwitch controller )
		{
			this.configuration = configuration;
			this.controller = controller;
		}

		protected override LoggerConfiguration CreateItem() => configuration.MinimumLevel.ControlledBy( controller );
	}

	public class RecordingLoggingConfigurationTransformer : TransformerBase<LoggerConfiguration>
	{
		readonly RecordingLogEventSink recorder;

		public RecordingLoggingConfigurationTransformer( [Required] RecordingLogEventSink recorder )
		{
			this.recorder = recorder;
		}

		protected override LoggerConfiguration CreateItem( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( recorder );
	}

	public class RecordingLoggerFactory : LoggingFactory
	{
		public RecordingLoggerFactory() : this( new RecordingLogEventSink(), new LoggingLevelSwitch() ) {}

		public RecordingLoggerFactory( [Required]RecordingLogEventSink sink, [Required]LoggingLevelSwitch levelSwitch ) : base( new RecordingLoggingConfigurationFactory( sink, levelSwitch ).Create )
		{
			Sink = sink;
			LevelSwitch = levelSwitch;
		}

		public virtual RecordingLogEventSink Sink { get; }
		public virtual LoggingLevelSwitch LevelSwitch { get; }

		/*protected override ILogger CreateItem()
		{
			var result = new LoggerConfiguration()
				.WriteTo.Sink( Sink )
				.MinimumLevel.ControlledBy( LevelSwitch )
				.CreateLogger();
			/*new LoggingProperties.AssociatedSink( result ).Assign( Sink );
			new LoggingProperties.AssociatedSwitch( result ).Assign( LevelSwitch );#1#
			return result;
		}*/
	}

	/*public static class LoggingProperties
	{
		public class AssociatedSink : AssociatedValue<ILogger, RecordingLogEventSink>
		{
			public AssociatedSink( ILogger instance ) : base( instance, typeof(AssociatedSink) ) {}
		}

		public class AssociatedSwitch : AssociatedValue<ILogger, Serilog.Core.LoggingLevelSwitch>
		{
			public AssociatedSwitch( ILogger instance ) : base( instance, typeof(AssociatedSwitch) ) {}
		}
	}*/
}
