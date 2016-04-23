using DragonSpark.Runtime;
using Dynamitey;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Diagnostics.Logger
{
	public delegate void LogTemplate( string template, object[] parameters );

	public delegate void LogException( Exception exception, string template, object[] parameters );

	public abstract class LogCommandBase<TDelegate, TTemplate> : Command<TTemplate> where TTemplate : ILoggerTemplate
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

		protected override void OnExecute( TTemplate parameter )
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

	public class LogTemplateCommand : LogCommandBase<LogTemplate, ILoggerTemplate>
	{
		public LogTemplateCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogTemplateCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}
		public LogTemplateCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, template => new object[] { template.Template, template.Parameters } ) {}
	}

	public class LogExceptionCommand : LogCommandBase<LogException, ILoggerExceptionTemplate>
	{
		public LogExceptionCommand( ILogger logger ) : this( logger, template => template.IntendedLevel ) {}
		public LogExceptionCommand( ILogger logger, LogEventLevel level ) : this( logger, template => level ) {}

		public LogExceptionCommand( ILogger logger, Func<ILoggerTemplate, LogEventLevel> levelSource ) : base( logger, levelSource, template => new object[] { template.Exception, template.Template, template.Parameters } ) {}
	}

	public class Handler<T> : DecoratedCommand<T, ILoggerTemplate>
	{
		public Handler( ILogger logger, LogEventLevel level, Func<T, ILoggerTemplate> transform ) : base( transform, new LogTemplateCommand( logger, level ) ) {}
	}
}