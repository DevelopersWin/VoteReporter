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

	public abstract class PurgeLoggerHistoryCommand<T> : CommandBase<Action<T>>
	{
		readonly ILoggerHistory history;
		readonly Func<IEnumerable<LogEvent>, T[]> factory;

		protected PurgeLoggerHistoryCommand( [Required] ILoggerHistory history, [Required] Func<IEnumerable<LogEvent>, T[]> factory )
		{
			this.history = history;
			this.factory = factory;
		}

		public override void Execute( Action<T> parameter )
		{
			var messages = factory( history.Events );
			messages.Each( parameter );
			history.Clear();
		}
	}

	public class RecordingLoggerConfigurationFactory : LoggerConfigurationFactory
	{
		public RecordingLoggerConfigurationFactory( [Required] ILoggerHistory sink, [Required] LoggingLevelSwitch controller, params ITransformer<LoggerConfiguration>[] transformers ) 
			: base( new LoggerConfigurationSource( controller ), transformers.Append( new LoggerHistoryConfigurationTransformer( sink ) ).Fixed() ) {}
	}

	public class MethodFormatter : IFormattable
	{
		readonly MethodBase method;

		public MethodFormatter( MethodBase method )
		{
			this.method = method;
		}

		public string ToString( string format, IFormatProvider formatProvider ) => $"{method.DeclaringType.Name}.{method.Name}";
	}

	public class LoggerConfigurationFactory : AggregateFactory<LoggerConfiguration>
	{
		public LoggerConfigurationFactory( IFactory<LoggerConfiguration> primary, params ITransformer<LoggerConfiguration>[] transformers ) : base( primary, transformers ) {}
	}

	public class LoggerConfigurationSource : FactoryBase<LoggerConfiguration>
	{
		readonly LoggerConfiguration configuration;
		readonly LoggingLevelSwitch controller;

		public LoggerConfigurationSource( [Required] LoggingLevelSwitch logging ) : this( new LoggerConfiguration(), logging ) {}

		public LoggerConfigurationSource( [Required] LoggerConfiguration configuration, [Required] LoggingLevelSwitch controller )
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

		public LoggingLevelSwitchFactory() : this( Configure.Get<MinimumLevelConfiguration, LogEventLevel> ) {}

		public LoggingLevelSwitchFactory( Func<LogEventLevel> source )
		{
			this.source = source;
		}

		protected override LoggingLevelSwitch CreateItem() => new LoggingLevelSwitch { MinimumLevel = source() };
	}

	public class RecordingLoggerFactory : LoggerFactory
	{
		public RecordingLoggerFactory() : this( Default<ITransformer<LoggerConfiguration>>.Items ) {}

		public RecordingLoggerFactory( params ITransformer<LoggerConfiguration>[] transformers ) : this( new LoggerHistorySink(), transformers ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, params ITransformer<LoggerConfiguration>[] transformers ) : this( history, LoggingLevelSwitchFactory.Instance.Create(), transformers ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, [Required]LoggingLevelSwitch levelSwitch, params ITransformer<LoggerConfiguration>[] transformers ) : this( history, levelSwitch, new RecordingLoggerConfigurationFactory( history, levelSwitch, transformers ).Create ) {}

		public RecordingLoggerFactory( [Required]ILoggerHistory history, [Required]LoggingLevelSwitch levelSwitch, Func<LoggerConfiguration> configuration ) : base( configuration )
		{
			History = history;
			LevelSwitch = levelSwitch;
		}

		public ILoggerHistory History { get; }
		public LoggingLevelSwitch LevelSwitch { get; }
	}
}
