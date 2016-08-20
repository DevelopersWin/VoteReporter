using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Diagnostics.Logging
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

	abstract class LoggerTemplateParameterFactoryBase<T> : ParameterizedSourceBase<T, object[]> where T : ILoggerTemplate
	{
		// public static TemplateParameterFactoryBase Instance { get; } = new TemplateParameterFactoryBase<T>();

		readonly Formatter source;

		protected LoggerTemplateParameterFactoryBase() : this( Formatter.Instance ) {}

		protected LoggerTemplateParameterFactoryBase( Formatter source )
		{
			this.source = source;
		}

		protected object[] Parameters( T parameter ) => parameter.Parameters.Select( source.Format ).ToArray();
	}

	class LoggerTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerTemplate>
	{
		public static LoggerTemplateParameterFactory Instance { get; } = new LoggerTemplateParameterFactory();

		public override object[] Get( ILoggerTemplate parameter ) => new object[] { parameter.Template, Parameters( parameter ) };
	}

	class LoggerExceptionTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerExceptionTemplate>
	{
		public static LoggerExceptionTemplateParameterFactory Instance { get; } = new LoggerExceptionTemplateParameterFactory();

		public override object[] Get( ILoggerExceptionTemplate parameter ) => new object[] { parameter.Exception, parameter.Template, Parameters( parameter ) };
	}

	public class LogTemplateCommand : LogCommandBase<LogTemplate, ILoggerTemplate>
	{
		public LogTemplateCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogTemplateCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}
		public LogTemplateCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerTemplateParameterFactory.Instance.Get ) {}
	}

	public class LogExceptionCommand : LogCommandBase<LogException, ILoggerExceptionTemplate>
	{
		public LogExceptionCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogExceptionCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}

		public LogExceptionCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerExceptionTemplateParameterFactory.Instance.Get ) {}
	}

	public sealed class Logger : ConfigurableParameterizedFactoryBase<LoggerConfiguration, ILogger>
	{
		public static IParameterizedSource<ILogger> Instance { get; } = new Logger().ToCache();
		Logger() : base( o => new LoggerConfiguration(), LoggerConfigurationSource.Instance.ToDelegate().Wrap(), ( configuration, parameter ) => configuration.CreateLogger().ForContext( Constants.SourceContextPropertyName, parameter, true ) ) {}
	}

	public sealed class LoggingHistory : Scope<LoggerHistorySink>
	{
		public static LoggingHistory Instance { get; } = new LoggingHistory();
		LoggingHistory() : base( Factory.Global( () => new LoggerHistorySink() ) ) {}
	}

	public sealed class LoggingController : Scope<LoggingLevelSwitch>
	{
		public static LoggingController Instance { get; } = new LoggingController();
		LoggingController() : base( Factory.Global( () => new LoggingLevelSwitch( MinimumLevelConfiguration.Instance.Get() ) ) ) {}
	}

	sealed class LoggerConfigurationSource : LoggerConfigurationSourceBase
	{
		public static LoggerConfigurationSource Instance { get; } = new LoggerConfigurationSource();
		LoggerConfigurationSource() : base( HistoryTransform.Instance ) {}

		sealed class HistoryTransform : TransformerBase<LoggerConfiguration>
		{
			public static HistoryTransform Instance { get; } = new HistoryTransform();
			HistoryTransform() : this( LoggingHistory.Instance.Get ) {}

			readonly Func<ILoggerHistory> history;

			HistoryTransform( Func<ILoggerHistory> history )
			{
				this.history = history;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history() );
		}
	}

	sealed class FormatterConfiguration : TransformerBase<LoggerConfiguration>
	{
		readonly static Func<object, object> Formatter = Diagnostics.Formatter.Instance.Format;

		public static FormatterConfiguration Instance { get; } = new FormatterConfiguration();
		FormatterConfiguration() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			foreach ( var type in KnownTypes.Instance.Get<IFormattable>() )
			{
				var located = ConstructingParameterLocator.Instance.Get( type );
				if ( located != null )
				{
					parameter.Destructure.ByTransformingWhere( new TypeAssignableSpecification( located ).IsSatisfiedBy, Formatter );
				}
			}

			return parameter;
		}
	}

	public abstract class LoggerConfigurationSourceBase : ConfigurationSource<LoggerConfiguration>
	{
		readonly static ITransformer<LoggerConfiguration> LogContext = EnrichFromLogContextCommand.Instance.ToTransformer();

		protected LoggerConfigurationSourceBase( params ITransformer<LoggerConfiguration>[] items ) : base( items.Fixed( LogContext, FormatterConfiguration.Instance, ControllerTransform.Instance, ApplicationAssemblyTransform.Instance ) ) {}

		sealed class ControllerTransform : TransformerBase<LoggerConfiguration>
		{
			public static ControllerTransform Instance { get; } = new ControllerTransform();
			ControllerTransform() : this( LoggingController.Instance.Get ) {}

			readonly Func<LoggingLevelSwitch> controller;

			ControllerTransform( Func<LoggingLevelSwitch> controller )
			{
				this.controller = controller;
			}

			public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.MinimumLevel.ControlledBy( controller() );
		}
	}

	sealed class ApplicationAssemblyTransform : TransformerBase<LoggerConfiguration>, ILogEventEnricher
	{
		public static ApplicationAssemblyTransform Instance { get; } = new ApplicationAssemblyTransform();
		ApplicationAssemblyTransform() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.Enrich.With( this );

		public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( nameof(AssemblyInformation), DefaultAssemblyInformationSource.Instance.Get(), true ) );
	}

	/*sealed class CreatorFilterTransformer : TransformerBase<LoggerConfiguration>
	{
		public static CreatorFilterTransformer Instance { get; } = new CreatorFilterTransformer();
		CreatorFilterTransformer() {}

		/*readonly CreatorFilter filter;

		public CreatorFilterTransformer() : this( new CreatorFilter() ) {}

		public CreatorFilterTransformer( CreatorFilter filter )
		{
			this.filter = filter;
		}#1#

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			var item = new CreatorFilter().ToItem();
			var result = parameter.Filter.With( item ).Enrich.With( item );
			return result;
		}

		sealed class CreatorFilter : SpecificationBase<LogEvent>, ILogEventEnricher, ILogEventFilter
		{
			const string CreatorId = nameof(CreatorId);
			readonly Guid id;

			public CreatorFilter() : this( Guid.NewGuid() ) {}

			CreatorFilter( Guid id )
			{
				this.id = id;
			}
			
			public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( CreatorId, id ) );

			public bool IsEnabled( LogEvent logEvent ) => IsSatisfiedBy( logEvent );

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
	}*/
}