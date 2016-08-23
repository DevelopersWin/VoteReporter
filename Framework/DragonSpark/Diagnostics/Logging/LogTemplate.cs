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

namespace DragonSpark.Diagnostics.Logging
{
	public delegate void LogTemplate<in T>( string template, T parameter );
	public delegate void LogTemplate<in T1, in T2>( string template, T1 first, T2 second );
	public delegate void LogTemplate<in T1, in T2, in T3>( string template, T1 first, T2 second, T3 third );
	public delegate void LogTemplate( string template, params object[] parameters );

	public abstract class LogCommandBase<T> : CommandBase<T>
	{
		readonly LogTemplate<T> action;
		readonly string messageTemplate;

		protected LogCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}

		protected LogCommandBase( LogTemplate<T> action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( T parameter ) => action( messageTemplate, parameter );
	}

	public abstract class LogCommandBase<T1, T2> : CommandBase<ValueTuple<T1, T2>>
	{
		readonly LogTemplate<T1, T2> action;
		readonly string messageTemplate;

		protected LogCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}

		protected LogCommandBase( LogTemplate<T1, T2> action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( ValueTuple<T1, T2> parameter ) => action( messageTemplate, parameter.Item1, parameter.Item2 );

		public void Execute( T1 first, T2 second ) => Execute( new ValueTuple<T1, T2>( first, second ) );
	}

	public abstract class LogCommandBase<T1, T2, T3> : CommandBase<ValueTuple<T1, T2, T3>>
	{
		readonly LogTemplate<T1, T2, T3> action;
		readonly string messageTemplate;

		protected LogCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}
		protected LogCommandBase( LogTemplate<T1, T2, T3> action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( ValueTuple<T1, T2, T3> parameter ) => action( messageTemplate, parameter.Item1, parameter.Item2, parameter.Item3 );

		public void Execute( T1 first, T2 second, T3 third ) => Execute( new ValueTuple<T1, T2, T3>( first, second, third ) );
	}

	public abstract class LogCommandBase : CommandBase<object[]>
	{
		readonly LogTemplate action;
		readonly string messageTemplate;

		protected LogCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}
		protected LogCommandBase( LogTemplate action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( object[] parameter ) => action( messageTemplate, parameter );

		public void ExecuteUsing( params object[] arguments ) => Execute( arguments );
	}

	public delegate void LogException<in T>( Exception exception, string template, T parameter );
	public delegate void LogException<in T1, in T2>( Exception exception, string template, T1 first, T2 second );
	public delegate void LogException<in T1, in T2, in T3>( Exception exception, string template, T1 first, T2 second, T3 third );
	public delegate void LogException( Exception exception, string template, params object[] parameters );

	public abstract class LogExceptionCommandBase<T> : CommandBase<ExceptionParameter<T>>
	{
		readonly LogException<T> action;
		readonly string messageTemplate;

		protected LogExceptionCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}
		protected LogExceptionCommandBase( LogException<T> action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( ExceptionParameter<T> parameter ) => action( parameter.Exception, messageTemplate, parameter.Argument );

		public void Execute( Exception exception, T argument ) => Execute( new ExceptionParameter<T>( exception, argument ) );
	}

	public abstract class LogExceptionCommandBase<T1, T2> : CommandBase<ExceptionParameter<ValueTuple<T1, T2>>>
	{
		readonly LogException<T1, T2> action;
		readonly string messageTemplate;

		protected LogExceptionCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}
		protected LogExceptionCommandBase( LogException<T1, T2> action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( ExceptionParameter<ValueTuple<T1, T2>> parameter ) => action( parameter.Exception, messageTemplate, parameter.Argument.Item1, parameter.Argument.Item2 );

		public void Execute( Exception exception, T1 first, T2 second ) => Execute( new ExceptionParameter<ValueTuple<T1, T2>>( exception, new ValueTuple<T1,T2>( first, second ) ) );
	}

	public abstract class LogExceptionCommandBase<T1, T2, T3> : CommandBase<ExceptionParameter<ValueTuple<T1, T2, T3>>>
	{
		readonly LogException<T1, T2, T3> action;
		readonly string messageTemplate;

		protected LogExceptionCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}
		protected LogExceptionCommandBase( LogException<T1, T2, T3> action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( ExceptionParameter<ValueTuple<T1, T2, T3>> parameter ) => action( parameter.Exception, messageTemplate, parameter.Argument.Item1, parameter.Argument.Item2, parameter.Argument.Item3 );

		public void Execute( Exception exception, T1 first, T2 second, T3 third ) => Execute( new ExceptionParameter<ValueTuple<T1, T2, T3>>( exception, new ValueTuple<T1,T2, T3>( first, second, third ) ) );
	}

	public abstract class LogExceptionCommandBase : CommandBase<ExceptionParameter<object[]>>
	{
		readonly LogException action;
		readonly string messageTemplate;

		protected LogExceptionCommandBase( ILogger logger, string messageTemplate ) : this( logger.Information, messageTemplate ) {}
		protected LogExceptionCommandBase( LogException action, string messageTemplate )

		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( ExceptionParameter<object[]> parameter ) => action( parameter.Exception, messageTemplate, parameter.Argument );

		public void Execute( Exception exception, params object[] arguments ) => Execute( new ExceptionParameter<object[]>( exception, arguments ) );
	}

	public struct ExceptionParameter<T>
	{
		public ExceptionParameter( Exception exception, T argument )
		{
			Exception = exception;
			Argument = argument;
		}

		public Exception Exception { get; }
		public T Argument { get; }
	}

	

	/*public abstract class LogCommandBase<TDelegate, TTemplate> : CommandBase<TTemplate> where TTemplate : ILoggerTemplate
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
		// public static TemplateParameterFactoryBase Default { get; } = new TemplateParameterFactoryBase<T>();

		readonly Formatter source;

		protected LoggerTemplateParameterFactoryBase() : this( Formatter.Default ) {}

		protected LoggerTemplateParameterFactoryBase( Formatter source )
		{
			this.source = source;
		}

		protected object[] Parameters( T parameter ) => parameter.Parameters.Select( source.Format ).ToArray();
	}

	class LoggerTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerTemplate>
	{
		public static LoggerTemplateParameterFactory Default { get; } = new LoggerTemplateParameterFactory();

		public override object[] Get( ILoggerTemplate parameter ) => new object[] { parameter.Template, Parameters( parameter ) };
	}

	class LoggerExceptionTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerExceptionTemplate>
	{
		public static LoggerExceptionTemplateParameterFactory Default { get; } = new LoggerExceptionTemplateParameterFactory();

		public override object[] Get( ILoggerExceptionTemplate parameter ) => new object[] { parameter.Exception, parameter.Template, Parameters( parameter ) };
	}

	public class LogTemplateCommand : LogCommandBase<LogTemplate, ILoggerTemplate>
	{
		public LogTemplateCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogTemplateCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}
		public LogTemplateCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerTemplateParameterFactory.Default.Get ) {}
	}

	public class LogExceptionCommand : LogCommandBase<LogException, ILoggerExceptionTemplate>
	{
		public LogExceptionCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogExceptionCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}

		public LogExceptionCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, LoggerExceptionTemplateParameterFactory.Default.Get ) {}
	}*/

	public sealed class Logger : ConfigurableParameterizedFactoryBase<LoggerConfiguration, ILogger>
	{
		public static IParameterizedSource<ILogger> Default { get; } = new Logger().ToCache();
		Logger() : base( o => new LoggerConfiguration(), LoggerConfigurationSource.Default.ToDelegate().Wrap(), ( configuration, parameter ) => configuration.CreateLogger().ForContext( Constants.SourceContextPropertyName, parameter, true ) ) {}
	}

	public sealed class LoggingHistory : Scope<LoggerHistorySink>
	{
		public static LoggingHistory Default { get; } = new LoggingHistory();
		LoggingHistory() : base( Factory.Global( () => new LoggerHistorySink() ) ) {}
	}

	public sealed class LoggingController : Scope<LoggingLevelSwitch>
	{
		public static LoggingController Default { get; } = new LoggingController();
		LoggingController() : base( Factory.Global( () => new LoggingLevelSwitch( MinimumLevelConfiguration.Default.Get() ) ) ) {}
	}

	sealed class LoggerConfigurationSource : LoggerConfigurationSourceBase
	{
		public static LoggerConfigurationSource Default { get; } = new LoggerConfigurationSource();
		LoggerConfigurationSource() : base( HistoryTransform.DefaultNested ) {}

		sealed class HistoryTransform : TransformerBase<LoggerConfiguration>
		{
			public static HistoryTransform DefaultNested { get; } = new HistoryTransform();
			HistoryTransform() : this( LoggingHistory.Default.Get ) {}

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
		readonly static Func<object, object> Formatter = Diagnostics.Formatter.Default.Format;

		public static FormatterConfiguration Default { get; } = new FormatterConfiguration();
		FormatterConfiguration() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			foreach ( var type in KnownTypes.Default.Get<IFormattable>() )
			{
				var located = ConstructingParameterLocator.Default.Get( type );
				if ( located != null )
				{
					parameter.Destructure.ByTransformingWhere( new TypeAssignableSpecification( located ).ToCachedSpecification().ToSpecificationDelegate(), Formatter );
				}
			}

			return parameter;
		}
	}

	public abstract class LoggerConfigurationSourceBase : ConfigurationSource<LoggerConfiguration>
	{
		readonly static ITransformer<LoggerConfiguration> LogContext = EnrichFromLogContextCommand.Default.ToTransformer();

		protected LoggerConfigurationSourceBase( params ITransformer<LoggerConfiguration>[] items ) : base( items.Fixed( LogContext, FormatterConfiguration.Default, ControllerTransform.Default, ApplicationAssemblyTransform.Default ) ) {}

		sealed class ControllerTransform : TransformerBase<LoggerConfiguration>
		{
			public static ControllerTransform Default { get; } = new ControllerTransform();
			ControllerTransform() : this( LoggingController.Default.Get ) {}

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
		public static ApplicationAssemblyTransform Default { get; } = new ApplicationAssemblyTransform();
		ApplicationAssemblyTransform() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.Enrich.With( this );

		public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( nameof(AssemblyInformation), DefaultAssemblyInformationSource.Default.Get(), true ) );
	}

	/*sealed class CreatorFilterTransformer : TransformerBase<LoggerConfiguration>
	{
		public static CreatorFilterTransformer Default { get; } = new CreatorFilterTransformer();
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