using DragonSpark.Runtime;
using System;

namespace DragonSpark.Diagnostics.Logger
{
	public delegate void Log( string template, object[] parameters );

	public class LogCommand : Command<LoggerTemplate>
	{
		readonly Log log;

		public LogCommand( Log log ) 
		{
			this.log = log;
		}

		protected override void OnExecute( LoggerTemplate parameter ) => log( parameter.Template, parameter.Parameters );
	}

	public class Handler<T> : DecoratedCommand<T, LoggerTemplate>
	{
		public Handler( Log log, Func<T, LoggerTemplate> transform ) : base( transform, new LogCommand( log ) ) {}
	}
}