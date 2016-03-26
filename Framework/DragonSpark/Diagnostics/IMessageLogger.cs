using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Markup;
using DragonSpark.Activation;

namespace DragonSpark.Diagnostics
{
	[ContentProperty( nameof(Commands) )]
	public class ConfiguringLoggerConfigurationFactory : TransformerBase<LoggerConfiguration>
	{
		public Collection<Command<LoggerConfiguration>> Commands { get; } = new Collection<Command<LoggerConfiguration>>();

		protected override LoggerConfiguration CreateItem( LoggerConfiguration configuration ) => Commands.Aggregate( configuration, ( loggerConfiguration, command ) => loggerConfiguration.With( command.Run ) );
	}

	public class AddTextWriterCommand : AddSinkCommand
	{
		public AddTextWriterCommand() : this( new StringWriter(), "{Timestamp} [{Level}] {Message}{NewLine}{Exception}" ) {}

		public AddTextWriterCommand( [Required]TextWriter writer, string outputTemplate )
		{
			Writer = writer;
			OutputTemplate = outputTemplate;
		}

		public TextWriter Writer { get; set; }

		public string OutputTemplate { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.TextWriter( Writer, RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}

	public abstract class AddSinkCommand : LoggerConfigurationCommandBase<LoggerSinkConfiguration>
	{
		protected AddSinkCommand() : this( LogEventLevel.Verbose ) {}

		protected AddSinkCommand( LogEventLevel restrictedToMinimumLevel ) : base( configuration => configuration.WriteTo )
		{
			RestrictedToMinimumLevel = restrictedToMinimumLevel;
		}

		public IFormatProvider FormatProvider { get; set; }

		public LogEventLevel RestrictedToMinimumLevel { get; set; }
	}

	public abstract class MinimumLevelCommandBase : LoggerConfigurationCommandBase<LoggerMinimumLevelConfiguration>
	{
		protected MinimumLevelCommandBase() : base( configuration => configuration.MinimumLevel ) {}
	}

	public class MinimumLevelSwitchCommand : MinimumLevelCommandBase
	{
		public MinimumLevelSwitchCommand() : this( LogEventLevel.Information ) {}

		public MinimumLevelSwitchCommand( LogEventLevel level ) : this( new Serilog.Core.LoggingLevelSwitch( level ) ) {}

		public MinimumLevelSwitchCommand( [Required]Serilog.Core.LoggingLevelSwitch controller )
		{
			Controller = controller;
		}

		[Required]
		public Serilog.Core.LoggingLevelSwitch Controller { [return: Required]get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.ControlledBy( Controller );
	}

	public abstract class ReadFromCommandBase : LoggerConfigurationCommandBase<LoggerSettingsConfiguration>
	{
		protected ReadFromCommandBase() : base( configuration => configuration.ReadFrom ) {}
	}

	public class ReadFromSettingsCommand : ReadFromCommandBase
	{
		public ReadFromSettingsCommand( [Required]ILoggerSettings settings )
		{
			Settings = settings;
		}

		[Required]
		public ILoggerSettings Settings { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.Settings( Settings );
	}

	public class ReadFromKeyValuePairsCommand : ReadFromCommandBase
	{
		public ReadFromKeyValuePairsCommand() : this( new Dictionary<string, string>() ) {}

		public ReadFromKeyValuePairsCommand( [Required]IDictionary<string, string> dictionary )
		{
			Dictionary = dictionary;
		}

		[Required]
		public IDictionary<string, string> Dictionary { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.KeyValuePairs( Dictionary );
	}

	public abstract class DestructureCommandBase : LoggerConfigurationCommandBase<LoggerDestructuringConfiguration>
	{
		protected DestructureCommandBase() : base( configuration => configuration.Destructure ) {}
	}

	[ContentProperty( nameof(Items) )]
	public class DestructureCommand : DestructureCommandBase
	{
		public Collection<IDestructuringPolicy> Items { get; } = new Collection<IDestructuringPolicy>();

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.With( Items.Fixed() );
	}

	public class DestructureTypeCommand : DestructureCommandBase
	{
		[Required]
		public Type ScalarType { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.AsScalar( ScalarType );
	}

	public class DestructureMaximumDepthCommand : DestructureCommandBase
	{
		[Range( 0, 100 )]
		public int MaximumDepth { get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ToMaximumDepth( MaximumDepth );
	}

	public abstract class DestructureByTranformingCommandBase<T> : DestructureCommandBase
	{
		protected DestructureByTranformingCommandBase( [Required]IFactory<T, object> factory )
		{
			Factory = factory;
		}

		[Required]
		public IFactory<T, object> Factory { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ByTransforming<T>( Factory.Create );
	}

	public abstract class FilterCommandBase : LoggerConfigurationCommandBase<LoggerFilterConfiguration>
	{
		protected FilterCommandBase() : base( configuration => configuration.Filter ) {}
	}

	[ContentProperty( nameof(Items) )]
	public class FilterCommand : FilterCommandBase
	{
		public Collection<ILogEventFilter> Items { get; } = new Collection<ILogEventFilter>();

		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.With( Items.Fixed() );
	}

	public class FilterByIncludingOnlyCommand : FilterBySpecificationCommandBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByIncludingOnly( Specification.IsSatisfiedBy );
	}

	public abstract class FilterBySpecificationCommandBase : FilterCommandBase
	{
		[Required]
		public ISpecification<LogEvent> Specification { [return: Required]get; set; }
	}

	public class FilterByExcludingCommand : FilterBySpecificationCommandBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByExcluding( Specification.IsSatisfiedBy );
	}

	[ContentProperty( nameof(Items) )]
	public class EnrichCommand : EnrichCommandBase
	{
		public Collection<ILogEventEnricher> Items { get; } = new Collection<ILogEventEnricher>();
		
		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.With( Items.Fixed() );
	}

	public class EnrichWithPropertyCommand : EnrichCommandBase
	{
		public string PropertyName { get; set; }

		public object Value { get; set; }

		public bool DestructureObjects { get; set; }

		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.WithProperty( PropertyName, Value, DestructureObjects );
	}

	public abstract class EnrichCommandBase : LoggerConfigurationCommandBase<LoggerEnrichmentConfiguration>
	{
		protected EnrichCommandBase() : base( configuration => configuration.Enrich ) {}
	}

	public class MinimumLevelIsCommand : MinimumLevelCommandBase
	{
		public MinimumLevelIsCommand() : this( LogEventLevel.Information ) {}

		public MinimumLevelIsCommand( LogEventLevel level )
		{
			Level = level;
		}

		public LogEventLevel Level { get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.Is( Level );
	}

	public abstract class LoggerConfigurationCommandBase<T> : SetupCommandBase<LoggerConfiguration>
	{
		readonly Func<LoggerConfiguration, T> transform;

		protected LoggerConfigurationCommandBase( Func<LoggerConfiguration, T> transform )
		{
			this.transform = transform;
		}

		protected sealed override void OnExecute( LoggerConfiguration parameter )
		{
			var transformed = transform( parameter );
			Configure( transformed );
		}

		protected abstract void Configure( T configuration );
	}

	/*public static class MessageLoggerExtensions
	{
		public static TLogger Information<TLogger>( this TLogger @this, string message, Priority priority = Priority.Normal ) where TLogger : IMessageLogger
		{
			new LogInformationCommand( @this ).Execute( new MessageParameter( message, priority ) );
			return @this;
		}

		public static TLogger Warning<TLogger>( this TLogger @this, string message, Priority priority = Priority.High ) where TLogger : IMessageLogger
		{
			new LogWarningCommand( @this ).Execute( new MessageParameter( message, priority ) );
			return @this;
		}

		public static TLogger Exception<TLogger>( this TLogger @this, string message, Exception exception ) where TLogger : IMessageLogger
		{
			new LogExceptionCommand( @this ).Execute( new ExceptionMessageParameter( message, exception ) );
			return @this;
		}

		public static TLogger Fatal<TLogger>( this TLogger @this, string message, FatalApplicationException exception ) where TLogger : IMessageLogger
		{
			new LogFatalExceptionCommand( @this ).Execute( new FatalExceptionMessageParameter( message, exception ) );
			return @this;
		}
	}

	public class LogExceptionCommand : LogMessageCommand<ExceptionMessageParameter>
	{
		public static LogExceptionCommand Instance { get; } = new LogExceptionCommand();

		public LogExceptionCommand() : this( LoggingServices.Instance ) { }

		public LogExceptionCommand( IMessageLogger logger ) : this( ExceptionMessageFactory.Instance, logger ) { }

		public LogExceptionCommand( IFactory<ExceptionMessageParameter, Message> factory, IMessageLogger logger ) : base( factory.Create, logger.Log ) { }
	}

	public class LogFatalExceptionCommand : LogMessageCommand<FatalExceptionMessageParameter>
	{
		public static LogFatalExceptionCommand Instance { get; } = new LogFatalExceptionCommand();

		public LogFatalExceptionCommand() : this( LoggingServices.Instance ) { }

		public LogFatalExceptionCommand( IMessageLogger logger ) : this( FatalExceptionMessageFactory.Instance, logger ) { }

		public LogFatalExceptionCommand( IFactory<FatalExceptionMessageParameter, Message> factory, IMessageLogger logger ) : base( factory.Create, logger.Log ) { }
	}

	public class LogWarningCommand : LogMessageCommand<MessageParameter>
	{
		public static LogWarningCommand Instance { get; } = new LogWarningCommand();

		public LogWarningCommand() : this( LoggingServices.Instance ) { }

		public LogWarningCommand( IMessageLogger logger ) : this( WarningMessageFactory.Instance, logger ) { }

		public LogWarningCommand( IFactory<MessageParameter, Message> factory, IMessageLogger logger ) : base( factory.Create, logger.Log ) { }
	}

	public class LogInformationCommand : LogMessageCommand<MessageParameter>
	{
		public static LogInformationCommand Instance { get; } = new LogInformationCommand();

		public LogInformationCommand() : this( LoggingServices.Instance ) {}

		public LogInformationCommand( IMessageLogger logger ) : this( InformationMessageFactory.Instance, logger ) {}

		public LogInformationCommand( IFactory<MessageParameter, Message> factory, IMessageLogger logger ) : base( factory.Create, logger.Log ) {}
	}

	public abstract class LogMessageCommand<T> : Command<T> where T : MessageParameter
	{
		readonly Func<T, Message> factory;
		readonly Action<Message> logger;

		protected LogMessageCommand( [Required]Func<T, Message> factory, [Required]Action<Message> logger )
		{
			this.factory = factory;
			this.logger = logger;
		}

		protected override void OnExecute( T parameter )
		{
			var message = factory( parameter );
			logger( message );
		}
	}

	public class FatalExceptionMessageFactory : ExceptionMessageFactoryBase<FatalExceptionMessageParameter>
	{
		public static FatalExceptionMessageFactory Instance { get; } = new FatalExceptionMessageFactory();

		public const string Category = "Fatal";

		public FatalExceptionMessageFactory() : this( CurrentTime.Instance, ExceptionFormatter.Instance ) { }

		public FatalExceptionMessageFactory( ICurrentTime time, IExceptionFormatter formatter ) : base( time, formatter, Category ) { }
	}

	public class ExceptionMessageFactory : ExceptionMessageFactoryBase<ExceptionMessageParameter>
	{
		public static ExceptionMessageFactory Instance { get; } = new ExceptionMessageFactory();

		public const string Category = "Exception";

		public ExceptionMessageFactory() : this( CurrentTime.Instance, ExceptionFormatter.Instance ) {}

		public ExceptionMessageFactory( ICurrentTime time, IExceptionFormatter formatter ) : base( time, formatter, Category ) {}
	}

	public abstract class ExceptionMessageFactoryBase<T> : MessageFactoryBase<T> where T : ExceptionMessageParameter
	{
		protected ExceptionMessageFactoryBase( ICurrentTime time, IExceptionFormatter formatter, string category ) : this( () => time.Now, formatter.Format, category ) { }

		protected ExceptionMessageFactoryBase( Func<DateTimeOffset> time, Func<Exception, string> format, string category ) : base( time, p => $"{p.Message} - {format( p.Exception )}", category ) {}
	}

	public class WarningMessageFactory : MessageFactoryBase<MessageParameter>
	{
		public const string Category = "Warning";

		public static WarningMessageFactory Instance { get; } = new WarningMessageFactory();

		public WarningMessageFactory() : base( Category )
		{}

		public WarningMessageFactory( Func<DateTimeOffset> time ) : base( time, Category )
		{}
	}

	public class InformationMessageFactory : MessageFactoryBase<MessageParameter>
	{
		public const string Category = "Information";

		public static InformationMessageFactory Instance { get; } = new InformationMessageFactory();

		public InformationMessageFactory() : base( Category )
		{}

		public InformationMessageFactory( Func<DateTimeOffset> time ) : base( time, Category )
		{}
	}

	public class FatalExceptionMessageParameter : ExceptionMessageParameter
	{
		public FatalExceptionMessageParameter( string message, FatalApplicationException exception, Priority priority = Priority.Highest ) : base( message, exception, priority )
		{}
	}

	public class ExceptionMessageParameter : MessageParameter
	{
		public ExceptionMessageParameter( [Required]string message, [Required]Exception exception, Priority priority = Priority.High ) : base( message, priority )
		{
			Exception = exception;
		}

		public Exception Exception { get; }
	}

	public class MessageParameter
	{
		public MessageParameter( string message, Priority priority )
		{
			Message = message;
			Priority = priority;
		}

		public string Message { get; }
		public Priority Priority { get; }
	}

	public abstract class MessageFactoryBase<T> : FactoryBase<T, Message> where T : MessageParameter
	{
		readonly Func<DateTimeOffset> time;
		readonly Func<T, string> message;
		readonly string category;

		protected MessageFactoryBase( string category ) : this( () => CurrentTime.Instance.Now, category ) {}

		protected MessageFactoryBase( Func<DateTimeOffset> time, string category ) : this( time, p => p.Message, category ) {}

		protected MessageFactoryBase( [Required]Func<DateTimeOffset> time, [Required]Func<T, string> message, [Required]string category )
		{
			this.time = time;
			this.message = message;
			this.category = category;
		}

		protected override Message CreateItem( T parameter )
		{
			var current = time();
			var formatted = string.Format( CultureInfo.InvariantCulture, Resources.DefaultTextLoggerPattern, current, category, message( parameter ), parameter.Priority );
			var result = new Message( parameter.Priority, current, category, formatted );
			return result;
		}
	}*/
}