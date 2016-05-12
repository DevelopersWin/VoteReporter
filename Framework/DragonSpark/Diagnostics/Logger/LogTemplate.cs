using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using Serilog;
using Serilog.Events;
using System;
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

		protected LoggerTemplateParameterFactoryBase() : this( Services.Get<FormatterFactory> ) {}

		protected LoggerTemplateParameterFactoryBase( Func<FormatterFactory> source )
		{
			this.source = source;
		}

		protected object[] Parameters( T parameter )
		{
			var formatter = source();
			var result = formatter.CreateMany( parameter.Parameters, Where<object>.Always );
			return result;
		}
	}

	class LoggerTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerTemplate>
	{
		public static LoggerTemplateParameterFactory Instance { get; } = new LoggerTemplateParameterFactory();

		protected override object[] CreateItem( ILoggerTemplate parameter ) => new object[] { parameter.Template, Parameters( parameter ) };
	}

	class LoggerExceptionTemplateParameterFactory : LoggerTemplateParameterFactoryBase<ILoggerExceptionTemplate>
	{
		public static LoggerExceptionTemplateParameterFactory Instance { get; } = new LoggerExceptionTemplateParameterFactory();

		protected override object[] CreateItem( ILoggerExceptionTemplate parameter ) => new object[] { parameter.Exception, parameter.Template, Parameters( parameter ) };
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
}