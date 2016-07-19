using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Diagnostics.Logger
{
	public delegate void LogTemplate( string template, params object[] parameters );

	public delegate void LogException( Exception exception, string template, params object[] parameters );

	public abstract class LogCommandBase<TDelegate, TTemplate> : CommandBase<TTemplate> where TTemplate : ILoggerTemplate
	{
		readonly ILogger logger;
		readonly Func<TTemplate, LogEventLevel> levelSource;
		readonly Func<TTemplate, object[]> parameterSource;

		protected LogCommandBase( ILogger logger, Func<TTemplate, LogEventLevel> levelSource, Func<TTemplate, object[]> parameterSource )
		{
			this.logger = logger;
			this.levelSource = levelSource;
			this.parameterSource = parameterSource;
		}

		public override void Execute( TTemplate parameter )
		{
			var parameters = parameterSource( parameter );
			var level = levelSource( parameter );
			var method = typeof(ILogger).GetRuntimeMethod( level.ToString(), parameters.Select( o => o.GetType() ).ToArray() );
			var @delegate = method.CreateDelegate( typeof(TDelegate), logger );
			@delegate.DynamicInvoke( parameters );
		}
	}

	public class LogCommand : FirstCommand<ILoggerTemplate>
	{
		public LogCommand( ILogger logger ) : base( new LogExceptionCommand( logger ), new LogTemplateCommand( logger ) ) {}
	}

	abstract class LoggerTemplateParameterFactoryBase<T> : FactoryBase<T, object[]> where T : ILoggerTemplate
	{
		// public static TemplateParameterFactoryBase Instance { get; } = new TemplateParameterFactoryBase<T>();

		readonly Func<FormatterFactory> source;

		protected LoggerTemplateParameterFactoryBase() : this( GlobalServiceProvider.Instance.GetService<FormatterFactory> ) {}

		protected LoggerTemplateParameterFactoryBase( Func<FormatterFactory> source )
		{
			this.source = source;
		}

		protected object[] Parameters( T parameter )
		{
			var formatter = source();
			var result =  parameter.Parameters.Select( formatter.From ).ToArray();
			return result;
		}
	}

	class LoggerTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerTemplate>
	{
		public static LoggerTemplateParameterFactory Instance { get; } = new LoggerTemplateParameterFactory();

		public override object[] Create( ILoggerTemplate parameter ) => new object[] { parameter.Template, Parameters( parameter ) };
	}

	class LoggerExceptionTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerExceptionTemplate>
	{
		public static LoggerExceptionTemplateParameterFactory Instance { get; } = new LoggerExceptionTemplateParameterFactory();

		public override object[] Create( ILoggerExceptionTemplate parameter ) => new object[] { parameter.Exception, parameter.Template, Parameters( parameter ) };
	}

	public class LogTemplateCommand : LogCommandBase<LogTemplate, ILoggerTemplate>
	{
		public LogTemplateCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogTemplateCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}
		public LogTemplateCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerTemplateParameterFactory.Instance.Create ) {}
	}

	public class LogExceptionCommand : LogCommandBase<LogException, ILoggerExceptionTemplate>
	{
		public LogExceptionCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogExceptionCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}

		public LogExceptionCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerExceptionTemplateParameterFactory.Instance.Create ) {}
	}

	/*public class Handler<T> : DecoratedCommand<T>
	{
		public Handler( ILogger logger, LogEventLevel level, Func<T, ILoggerTemplate> projection ) : base( new LogTemplateCommand( logger, level ).Cast( projection ) ) {}
	}*/

	public sealed class Logging : AggregateParameterizedFactoryBase<LoggerConfiguration, ILogger>
	{
		public static ICache<object, ILogger> Instance { get; } = new Logging().Cached();
		Logging() : base( o => new LoggerConfiguration(), LoggerConfigurationsFactory.Instance.Create, ( configuration, parameter ) => configuration.CreateLogger().ForSource( parameter ) ) {}
	}

	public sealed class LoggingHistory : ActivatedCache<LoggerHistorySink>
	{
		public new static LoggingHistory Instance { get; } = new LoggingHistory();
		LoggingHistory() {}
	}

	public sealed class LoggingController : Cache<LoggingLevelSwitch>
	{
		public static LoggingController Instance { get; } = new LoggingController();
		LoggingController() : base( o => new LoggingLevelSwitch( MinimumLevelConfiguration.Instance.Get( o ) ) ) {}
	}

	sealed class LoggerConfigurationsFactory : LoggerConfigurationsFactoryBase
	{
		public static LoggerConfigurationsFactory Instance { get; } = new LoggerConfigurationsFactory();

		protected override IEnumerable<ITransformer<LoggerConfiguration>> From( object parameter )
		{
			foreach ( var transformer in base.From( parameter ) )
			{
				yield return transformer;
			}

			yield return new HistoryTransform( LoggingHistory.Instance.Get( parameter ) );
		}

		class HistoryTransform : TransformerBase<LoggerConfiguration>
		{
			readonly ILoggerHistory history;

			public HistoryTransform( ILoggerHistory history )
			{
				this.history = history;
			}

			public override LoggerConfiguration Create( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history );
		}
	}

	public abstract class LoggerConfigurationsFactoryBase : FactoryBase<object, ImmutableArray<ITransformer<LoggerConfiguration>>>
	{
		// public static LoggerConfigurationsFactoryBase Instance { get; } = new LoggerConfigurationsFactoryBase();
		// protected LoggerConfigurationsFactoryBase() {}

		public override ImmutableArray<ITransformer<LoggerConfiguration>> Create( object parameter ) => From( parameter ).ToImmutableArray();

		protected virtual IEnumerable<ITransformer<LoggerConfiguration>> From( object parameter )
		{
			yield return new ControllerTransform( LoggingController.Instance.Get( parameter ) );
			yield return new CreatorFilterTransformer();
		}

		sealed class ControllerTransform : TransformerBase<LoggerConfiguration>
		{
			readonly LoggingLevelSwitch controller;
			public ControllerTransform( LoggingLevelSwitch controller )
			{
				this.controller = controller;
			}

			public override LoggerConfiguration Create( LoggerConfiguration parameter ) => parameter.MinimumLevel.ControlledBy( controller );
		}
	}

	public sealed class CreatorFilterTransformer : TransformerBase<LoggerConfiguration>
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
			var result = parameter.Filter.With( item ).Enrich.With( item );
			return result;
		}

		public sealed class CreatorFilter : DecoratedSpecification<LogEvent>, ILogEventEnricher, ILogEventFilter
		{
			const string CreatorId = "CreatorId";
			readonly Guid id;

			public CreatorFilter() : this( Guid.NewGuid() ) {}

			public CreatorFilter( Guid id ) : base( new CreatorSpecification( id ) )
			{
				this.id = id;
			}
			
			public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( CreatorId, id ) );

			/*class MigratingSpecification : CacheValueSpecification<LogEvent, bool>
			{
				public static MigratingSpecification Instance { get; } = new MigratingSpecification();

				MigratingSpecification() : base( MigrationProperties.IsMigrating, () => true ) {}
			}*/

			sealed class CreatorSpecification : SpecificationBase<LogEvent>
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
}