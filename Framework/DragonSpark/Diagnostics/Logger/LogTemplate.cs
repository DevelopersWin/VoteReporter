using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
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

	public class Handler<T> : DecoratedCommand<T>
	{
		public Handler( ILogger logger, LogEventLevel level, Func<T, ILoggerTemplate> projection ) : base( new LogTemplateCommand( logger, level ).Cast( projection ) ) {}
	}

	public sealed class LoggerHistory : ParameterizedConfiguration<ILoggerHistory>
	{
		public static LoggerHistory Instance { get; } = new LoggerHistory();
		LoggerHistory() : base( o => new LoggerHistorySink() ) {}
	}

	public sealed class LoggingLevelSwitch : ParameterizedConfiguration<Serilog.Core.LoggingLevelSwitch>
	{
		public static LoggingLevelSwitch Instance { get; } = new LoggingLevelSwitch();
		LoggingLevelSwitch() : base( o => new Serilog.Core.LoggingLevelSwitch( MinimumLevelConfiguration.Instance.Get( o ) ) ) {}
	}

	public sealed class LoggerConfiguration : ParameterizedConfiguration<Serilog.LoggerConfiguration>
	{
		public static LoggerConfiguration Instance { get; } = new LoggerConfiguration();
		LoggerConfiguration() : base( o => LoggerConfigurations.Instance.Get( o ).Aggregate( new Serilog.LoggerConfiguration(), ( configuration, transformer ) => transformer.Create( configuration ) ) ) {}
	}

	public sealed class Logger : ParameterizedConfiguration<ILogger>
	{
		public static Logger Instance { get; } = new Logger();
		Logger() : base( o => LoggerConfiguration.Instance.Get( o ).CreateLogger().ForSource( o ) ) {}
	}

	public sealed class LoggerConfigurations : ParameterizedConfiguration<ITransformer<Serilog.LoggerConfiguration>[]>
	{
		public static LoggerConfigurations Instance { get; } = new LoggerConfigurations();
		LoggerConfigurations() : base( RecordingLoggerConfigurationsFactory.Instance.Create ) {}
	}

	class RecordingLoggerConfigurationsFactory : LoggerConfigurationsFactory
	{
		public new static RecordingLoggerConfigurationsFactory Instance { get; } = new RecordingLoggerConfigurationsFactory();

		protected override IEnumerable<ITransformer<Serilog.LoggerConfiguration>> From( object parameter )
		{
			foreach ( var transformer in base.From( parameter ) )
			{
				yield return transformer;
			}

			yield return new HistoryTransform( LoggerHistory.Instance.Get( parameter ) );
		}

		class HistoryTransform : TransformerBase<Serilog.LoggerConfiguration>
		{
			readonly ILoggerHistory history;

			public HistoryTransform( ILoggerHistory history )
			{
				this.history = history;
			}

			public override Serilog.LoggerConfiguration Create( Serilog.LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history );
		}
	}

	public class LoggerConfigurationsFactory : FactoryBase<object, ITransformer<Serilog.LoggerConfiguration>[]>
	{
		public static LoggerConfigurationsFactory Instance { get; } = new LoggerConfigurationsFactory();
		protected LoggerConfigurationsFactory() {}

		public override ITransformer<Serilog.LoggerConfiguration>[] Create( object parameter ) => From( parameter ).ToArray();

		protected virtual IEnumerable<ITransformer<Serilog.LoggerConfiguration>> From( object parameter )
		{
			yield return new ControllerTransform( LoggingLevelSwitch.Instance.Get( parameter ) );
			yield return new CreatorFilterTransformer();
		}

		class ControllerTransform : TransformerBase<Serilog.LoggerConfiguration>
		{
			readonly Serilog.Core.LoggingLevelSwitch controller;
			public ControllerTransform( Serilog.Core.LoggingLevelSwitch controller )
			{
				this.controller = controller;
			}

			public override Serilog.LoggerConfiguration Create( Serilog.LoggerConfiguration parameter ) => parameter.MinimumLevel.ControlledBy( controller );
		}
	}

	public class CreatorFilterTransformer : TransformerBase<Serilog.LoggerConfiguration>
	{
		readonly CreatorFilter filter;

		public CreatorFilterTransformer() : this( new CreatorFilter() ) {}

		public CreatorFilterTransformer( CreatorFilter filter )
		{
			this.filter = filter;
		}

		public override Serilog.LoggerConfiguration Create( Serilog.LoggerConfiguration parameter )
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
}