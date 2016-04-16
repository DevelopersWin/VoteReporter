using DragonSpark.Activation;
using DragonSpark.Runtime;
using Serilog;
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

	public static class Category
	{
		public abstract class Factory : FactoryBase<ILogger, Log> {}

		public class Debug : Factory
		{
			public static Debug Instance { get; } = new Debug();

			protected override Log CreateItem( ILogger parameter ) => parameter.Debug;
		}

		public class Information : Factory
		{
			public static Information Instance { get; } = new Information();

			protected override Log CreateItem( ILogger parameter ) => parameter.Information;
		}

		public class Warning : Factory
		{
			public static Warning Instance { get; } = new Warning();

			protected override Log CreateItem( ILogger parameter ) => parameter.Warning;
		}

		public class Error : Factory
		{
			public static Error Instance { get; } = new Error();

			protected override Log CreateItem( ILogger parameter ) => parameter.Error;
		}

		public class Fatal : Factory
		{
			public static Fatal Instance { get; } = new Fatal();

			protected override Log CreateItem( ILogger parameter ) => parameter.Fatal;
		}
	}
}