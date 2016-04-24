using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Reflection;

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

	public class RecordingLoggingConfigurationFactory : AggregateFactory<LoggerConfiguration>
	{
		public RecordingLoggingConfigurationFactory( [Required] ILoggerHistory sink, [Required] LoggingLevelSwitch controller, params ITransformer<LoggerConfiguration>[] transformers ) 
			: base( new LoggerConfigurationFactory( controller ), transformers.Append( new LoggerHistoryConfigurationTransformer( sink ) ).Fixed() ) {}
	}

	public class MethodFormatter : FactoryBase<MethodBase, string>
	{
		public static MethodFormatter Instance { get; } = new MethodFormatter();

		protected override string CreateItem( MethodBase parameter ) => $"{parameter.DeclaringType.Name}.{parameter.Name}";
	}

	public class LoggerFactory : FactoryBase<ILogger>
	{
		readonly Func<LoggerConfiguration> source;

		public LoggerFactory( [Required] Func<LoggerConfiguration> source )
		{
			this.source = source;
		}

		protected override ILogger CreateItem() => source().CreateLogger().ForContext( Constants.SourceContextPropertyName, Execution.Current );
	}

	public class LoggerConfigurationFactory : FactoryBase<LoggerConfiguration>
	{
		readonly LoggerConfiguration configuration;
		readonly LoggingLevelSwitch controller;

		public LoggerConfigurationFactory( [Required] LoggingLevelSwitch logging ) : this( new LoggerConfiguration(), logging ) {}

		public LoggerConfigurationFactory( [Required] LoggerConfiguration configuration, [Required] LoggingLevelSwitch controller )
		{
			this.configuration = configuration;
			this.controller = controller;
		}

		protected override LoggerConfiguration CreateItem() => configuration.MinimumLevel.ControlledBy( controller );
	}

	public class LoggerHistoryConfigurationTransformer : TransformerBase<LoggerConfiguration>
	{
		readonly ILoggerHistory history;

		public LoggerHistoryConfigurationTransformer( [Required] ILoggerHistory history )
		{
			this.history = history;
		}

		protected override LoggerConfiguration CreateItem( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history );
	}

	public class LoggingLevelSwitchFactory : FactoryBase<LoggingLevelSwitch>
	{
		public static LoggingLevelSwitchFactory Instance { get; } = new LoggingLevelSwitchFactory();

		readonly Func<LogEventLevel> source;

		public LoggingLevelSwitchFactory() : this( Load<MinimumLevelConfiguration, LogEventLevel>.Get ) {}

		public LoggingLevelSwitchFactory( Func<LogEventLevel> source )
		{
			this.source = source;
		}

		protected override LoggingLevelSwitch CreateItem()
		{
			var logEventLevel = source();
			if ( Execution.Current.AsTo<MethodBase, bool>( method => method.DeclaringType != typeof(Services), () => false )  && logEventLevel == LogEventLevel.Information )
			{
				throw new InvalidOperationException( $"WTF: {Execution.Current}" );
			}
			return new LoggingLevelSwitch { MinimumLevel = logEventLevel };
		}
	}

	public class RecordingLoggerFactory : LoggerFactory
	{
		public RecordingLoggerFactory() : this( Default<ITransformer<LoggerConfiguration>>.Items ) {}

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
