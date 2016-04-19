using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace DragonSpark.Diagnostics
{
	public interface ILoggerHistory : ILogEventSink
	{
		IEnumerable<LogEvent> Events { get; }

		void Clear();
	}

	
	public class PurgeLoggerMessageHistoryCommand : PurgeLoggerHistoryCommand<string>
	{
		public PurgeLoggerMessageHistoryCommand( ILoggerHistory history ) : base( history, LogEventMessageFactory.Instance.Create ) {}
	}

	/*public class PurgeLoggerHistoryFixedCommand : FixedCommand
	{
		public PurgeLoggerHistoryFixedCommand( [Required] ILoggerHistory history, [Required] Action<string> output ) : base( new PurgeLoggerMessageHistoryCommand( history ), output ) {}
	}*/

	/*public class PurgeLoggerHistoryCommand : PurgeLoggerHistoryCommand<LogEvent>
	{
		public PurgeLoggerHistoryCommand( ILoggerHistory history ) : base( history, events => events.Fixed() ) {}
	}*/

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

	public class RecordingLoggingConfigurationFactory : AggregateFactory<LoggerConfiguration>
	{
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

		protected override ILogger CreateItem() => source().CreateLogger().ForContext( Constants.SourceContextPropertyName, Execution.Current.AsString() );
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

	public class Configuration
	{
		public static Configuration Default { get; } = new Configuration();

		static Configuration()
		{
			Initialize( typeof(ProfilerFactory<Category.Debug>) );
			Initialize( LogEventLevel.Information );
		}

		public static void Initialize( [OfFactoryType]Type defaultFactoryType ) => Default.ProfilerFactoryType = defaultFactoryType;

		public static void Initialize( LogEventLevel level ) => Default.Level = level;

		public Type ProfilerFactoryType { get; private set; }

		public LogEventLevel Level { get; private set; }
	}

	public class LoggingLevelSwitchFactory : FactoryBase<LoggingLevelSwitch>
	{
		public static LoggingLevelSwitchFactory Instance { get; } = new LoggingLevelSwitchFactory();

		protected override LoggingLevelSwitch CreateItem() => new LoggingLevelSwitch { MinimumLevel = Configuration.Default.Level };
	}

	public class RecordingLoggerFactory : LoggerFactory
	{
		public RecordingLoggerFactory( params ITransformer<LoggerConfiguration>[] transformers ) : this( new LoggerHistorySink(), transformers ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, params ITransformer<LoggerConfiguration>[] transformers ) : this( history, LoggingLevelSwitchFactory.Instance.Create(), transformers ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, [Required]LoggingLevelSwitch levelSwitch, params ITransformer<LoggerConfiguration>[] transformers ) : this( history, levelSwitch, new RecordingLoggingConfigurationFactory( history, levelSwitch, transformers ).Create ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, [Required]LoggingLevelSwitch levelSwitch, Func<LoggerConfiguration> configuration ) : base( configuration )
		{
			History = history;
			LevelSwitch = levelSwitch;
		}

		public ILoggerHistory History { get; }
		public LoggingLevelSwitch LevelSwitch { get; }
	}
}
