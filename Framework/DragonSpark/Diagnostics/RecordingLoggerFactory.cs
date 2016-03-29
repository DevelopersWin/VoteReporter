using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Composition;
using DragonSpark.Activation;

namespace DragonSpark.Diagnostics
{
	public class DiagnosticsFactory : FactoryBase<IDiagnostics>
	{
		readonly Func<ILogger> source;
		readonly LoggingLevelSwitch levelSwitch;

		/*public DiagnosticsFactory() : this( new RecordingLoggerFactory() ) {}

		DiagnosticsFactory( RecordingLoggerFactory factory ) : this( factory.Create, factory.LevelSwitch ) { }*/

		public DiagnosticsFactory( [Required] Func<ILogger> source, [Required]LoggingLevelSwitch levelSwitch )
		{
			this.source = source;
			this.levelSwitch = levelSwitch;
		}

		protected override IDiagnostics CreateItem()
		{
			var logger = source();
			var result = new Diagnostics( logger, levelSwitch );
			return result;
		}
	}

	public interface ILoggerHistory : ILogEventSink
	{
		IEnumerable<LogEvent> Events { get; }

		void Clear();
	}

	public class PurgeLoggerMessageHistoryCommand : PurgeLoggerHistoryCommand<string>
	{
		[ImportingConstructor]
		public PurgeLoggerMessageHistoryCommand( ILoggerHistory history ) : base( history, LogEventMessageFactory.Instance.Create ) {}
	}

	public class PurgeLoggerHistoryCommand : PurgeLoggerHistoryCommand<LogEvent>
	{
		public PurgeLoggerHistoryCommand( ILoggerHistory history ) : base( history, events => events.Fixed() ) {}
	}

	public abstract class PurgeLoggerHistoryCommand<T> : Command<Action<T>>
	{
		readonly ILoggerHistory history;
		readonly Func<IEnumerable<LogEvent>, T[]> factory;

		protected PurgeLoggerHistoryCommand( [Required] ILoggerHistory history, [Required] Func<IEnumerable<LogEvent>, T[]> factory )
		{
			this.history = history;
			this.factory = factory;
		}

		protected override void OnExecute( Action<T> parameter )
		{
			var messages = factory( history.Events );
			messages.Each( parameter );
			history.Clear();
		}
	}

	public interface IDiagnostics
	{
		ILogger Logger { get; }

		LoggingLevelSwitch Switch { get; }
	}

	public class RecordingLoggingConfigurationFactory : AggregateFactory<LoggerConfiguration>
	{
		// public RecordingLoggingConfigurationFactory() : this( new RecordingLogEventSink(), new LoggingLevelSwitch() ) {}

		public RecordingLoggingConfigurationFactory( [Required] ILoggerHistory sink, [Required] LoggingLevelSwitch controller, params ITransformer<LoggerConfiguration>[] transformers ) 
			: base( new LoggingConfigurationSourceFactory( controller ), transformers.Append( new RecordingLoggingConfigurationTransformer( sink ) ).Fixed() ) {}
	}

	public class LoggerFactory : FactoryBase<ILogger>
	{
		readonly Func<LoggerConfiguration> source;

		public LoggerFactory( [Required] Func<LoggerConfiguration> source )
		{
			this.source = source;
		}

		protected override ILogger CreateItem() => source().CreateLogger().ForContext( Constants.SourceContextPropertyName, "Default" );
	}

	public class LoggingConfigurationSourceFactory : FactoryBase<LoggerConfiguration>
	{
		readonly LoggerConfiguration configuration;
		readonly LoggingLevelSwitch controller;

		public LoggingConfigurationSourceFactory( [Required] LoggingLevelSwitch logging ) : this( new LoggerConfiguration(), logging ) {}

		public LoggingConfigurationSourceFactory( [Required] LoggerConfiguration configuration, [Required] LoggingLevelSwitch controller )
		{
			this.configuration = configuration;
			this.controller = controller;
		}

		protected override LoggerConfiguration CreateItem() => configuration.MinimumLevel.ControlledBy( controller );
	}

	public class RecordingLoggingConfigurationTransformer : TransformerBase<LoggerConfiguration>
	{
		readonly ILoggerHistory recorder;

		public RecordingLoggingConfigurationTransformer( [Required] ILoggerHistory recorder )
		{
			this.recorder = recorder;
		}

		protected override LoggerConfiguration CreateItem( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( recorder );
	}

	public class RecordingLoggerFactory : LoggerFactory
	{
		public RecordingLoggerFactory() : this( new LoggerHistorySink(), new LoggingLevelSwitch() ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, [Required]LoggingLevelSwitch levelSwitch ) : this( history, levelSwitch, new RecordingLoggingConfigurationFactory( history, levelSwitch ).Create ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, [Required]LoggingLevelSwitch levelSwitch, Func<LoggerConfiguration> configuration ) : base( configuration )
		{
			History = history;
			LevelSwitch = levelSwitch;
		}

		public ILoggerHistory History { get; }
		public LoggingLevelSwitch LevelSwitch { get; }
	}
}
