using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Configure = DragonSpark.Configuration.Configure;

namespace DragonSpark.Diagnostics
{
	public interface ILoggerHistory : ILogEventSink
	{
		IEnumerable<LogEvent> Events { get; }

		void Clear();
	}
	
	public class PurgeLoggerMessageHistoryCommand : PurgeLoggerHistoryCommand<string>
	{
		public PurgeLoggerMessageHistoryCommand( ILoggerHistory history ) : base( history, LogEventMessageFactory.Instance.ToDelegate() ) {}
	}

	public static class MigrationProperties
	{
		public static ICache<LogEvent, bool> IsMigrating { get; } = new StoreCache<LogEvent, bool>();
	}

	public class PurgeLoggerHistoryCommand : PurgeLoggerHistoryCommand<LogEvent>
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
			: base( new LoggerConfigurationSource( controller ).ToDelegate(), transformers.Append( new CreatorFilterTransformer(), new LoggerHistoryConfigurationTransformer( sink ) ).Select( transformer => transformer.ToDelegate() ).ToArray() ) {}
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
	}

	public class CreatorFilterTransformer : TransformerBase<LoggerConfiguration>
	{
		readonly CreatorFilter filter;

		public CreatorFilterTransformer() : this( new CreatorFilter() ) {}

		public CreatorFilterTransformer( CreatorFilter filter )
		{
			this.filter = filter;
		}

		public override LoggerConfiguration Create( LoggerConfiguration parameter )
		{
			var item = filter.ToItem();
			return parameter.Filter.With( item ).Enrich.With( item );
		}

		public class CreatorFilter : DecoratedSpecification<LogEvent>, ILogEventEnricher, ILogEventFilter
		{
			const string CreatorId = "CreatorId";
			readonly Guid id;

			public CreatorFilter() : this( Guid.NewGuid() ) {}

			public CreatorFilter( Guid id ) : base( MigratingSpecification.Instance.Or( new CreatorSpecification( id ) ) )
			{
				this.id = id;
			}
			
			public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( CreatorId, id ) );

			class MigratingSpecification : CacheValueSpecification<LogEvent, bool>
			{
				public static MigratingSpecification Instance { get; } = new MigratingSpecification();

				MigratingSpecification() : base( MigrationProperties.IsMigrating, () => true ) {}
			}

			class CreatorSpecification : SpecificationBase<LogEvent>
			{
				readonly Guid id;
				public CreatorSpecification( Guid id )
				{
					this.id = id;
				}

				public override bool IsSatisfiedBy( LogEvent parameter )
				{
					if ( parameter.Properties.ContainsKey( CreatorId ) )
					{
						var property = parameter.Properties[CreatorId] as ScalarValue;
						var result = property != null && (Guid)property.Value == id;
						return result;
					}

					return false;
				}
			}

			public bool IsEnabled( LogEvent logEvent ) => IsSatisfiedBy( logEvent );
		}
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

		public override LoggingLevelSwitch Create() => new LoggingLevelSwitch { MinimumLevel = source() };
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
	}
}
