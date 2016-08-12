using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Diagnostics
{
	public interface ILoggerHistory : ILogEventSink
	{
		IEnumerable<LogEvent> Events { get; }

		void Clear();
	}
	
	public class PurgeLoggerMessageHistoryCommand : PurgeLoggerHistoryCommand<string>
	{
		readonly static Func<IEnumerable<LogEvent>, ImmutableArray<string>> MessageFactory = LogEventMessageFactory.Instance.ToDelegate();

		public PurgeLoggerMessageHistoryCommand( ILoggerHistory history ) : base( history, MessageFactory ) {}
	}

	/*public static class MigrationProperties
	{
		public static ICache<LogEvent, bool> IsMigrating { get; } = new SourceCache<LogEvent, bool>();
	}*/

	/*public class PurgeLoggerHistoryCommand : PurgeLoggerHistoryCommand<LogEvent>
	{
		public PurgeLoggerHistoryCommand( ILoggerHistory history ) : base( history, events => events.Fixed() ) {}

		public override void Execute( Action<LogEvent> parameter ) => base.Execute( new Migrater( parameter ).Execute );

		class Migrater
		{
			readonly Action<LogEvent> action;

			public Migrater( Action<LogEvent> action )
			{
				this.action = action;
			}

			public void Execute( LogEvent parameter )
			{
				using ( MigrationProperties.IsMigrating.Assignment( parameter, true ) )
				{
					action( parameter );
				}
			}
		}
	}*/

	public abstract class PurgeLoggerHistoryCommand<T> : CommandBase<Action<T>>
	{
		readonly ILoggerHistory history;
		readonly Func<IEnumerable<LogEvent>, ImmutableArray<T>> factory;

		protected PurgeLoggerHistoryCommand( [Required] ILoggerHistory history, [Required] Func<IEnumerable<LogEvent>, ImmutableArray<T>> factory )
		{
			this.history = history;
			this.factory = factory;
		}

		public override void Execute( Action<T> parameter )
		{
			var messages = factory( history.Events ).ToArray();
			messages.Each( parameter );
			history.Clear();
		}
	}

	/*public class RecordingLoggerConfigurationFactory : LoggerConfigurationFactory
	{
		public RecordingLoggerConfigurationFactory( [Required] ILoggerHistory sink, [Required] LoggingLevelSwitch controller, params ITransformer<LoggerConfiguration>[] transformers ) 
			: base( new LoggerConfigurationSource( controller ).Create, transformers.Append( new CreatorFilterTransformer(), new LoggerHistoryConfigurationTransformer( sink ) ).Select( transformer => transformer.ToDelegate() ).ToArray() ) {}
	}*/

	public class MethodFormatter : IFormattable
	{
		readonly MethodBase method;

		public MethodFormatter( MethodBase method )
		{
			this.method = method;
		}

		public string ToString( string format, IFormatProvider formatProvider ) => $"{method.DeclaringType.Name}.{method.Name}";
	}

	/*public class LoggerConfigurationFactory : AggregateFactory<LoggerConfiguration>
	{
		public LoggerConfigurationFactory( Func<LoggerConfiguration> primary, params Func<LoggerConfiguration, LoggerConfiguration>[] transformers ) : base( primary, transformers ) {}
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

		public override LoggerConfiguration Create() => configuration.MinimumLevel.ControlledBy( controller );
	}

	public class LoggerHistoryConfigurationTransformer : TransformerBase<LoggerConfiguration>
	{
		readonly ILoggerHistory history;

		public LoggerHistoryConfigurationTransformer( [Required] ILoggerHistory history )
		{
			this.history = history;
		}

		public override LoggerConfiguration Create( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history );
	}*/

		

	/*public class LoggingLevelSwitchFactory : FactoryBase<LoggingLevelSwitch>
	{
		public static LoggingLevelSwitchFactory Instance { get; } = new LoggingLevelSwitchFactory();

		public override LoggingLevelSwitch Create() => new LoggingLevelSwitch { MinimumLevel = MinimumLevelConfiguration.Instance.Default() };
	}

	public class RecordingLoggerFactory : LoggerFactory
	{
		public RecordingLoggerFactory() : this( Items<ITransformer<LoggerConfiguration>>.Default ) {}

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
	}*/
}
