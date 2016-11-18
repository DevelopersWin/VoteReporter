using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

namespace DragonSpark.Diagnostics
{
	public sealed class Logger : ParameterizedSourceBase<ILogger>
	{
		public static IParameterizedSource<object, ILogger> Default { get; } = new ParameterizedSingletonScope<object, ILogger>( o => new Logger().Get );
		Logger() : this( LoggerFactory.Default.Get(), new DelegatedAssignedSpecification<object, IFormattable>( Formatters.Default.Get ).IsSatisfiedBy ) {}

		readonly ILogger logger;
		readonly Func<object, bool> specification;

		[UsedImplicitly]
		public Logger( ILogger logger, Func<object, bool> specification )
		{
			this.logger = logger;
			this.specification = specification;
		}

		public override ILogger Get( object parameter ) => logger.ForContext( Constants.SourceContextPropertyName, parameter, specification( parameter ) );
	}

	public sealed class DefaultLogger : DelegatedSource<ILogger>
	{
		public static DefaultLogger Default { get; } = new DefaultLogger();
		DefaultLogger() : base( Logger.Default.GetDefault ) {}
	}

	public sealed class LoggerFactory : ConfiguringFactory<ILogger>
	{
		public static LoggerFactory Default { get; } = new LoggerFactory();
		LoggerFactory() : base( Factory.Instance.Get, Command.Instance.Execute ) {}

		public sealed class Factory : Scope<ILogger>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() : this( LoggerConfigurationSource.Default ) {}

			public Factory( ISource<LoggerConfiguration> configuration ) : base( configuration.Into( CreateLogger.Instance ).Into( RegisterForDispose<ILogger>.Default ).Get ) {}
		}

		[ApplyAutoValidation, ApplySpecification( typeof(OncePerScopeSpecification<ILogger>) )]
		public sealed class Command : CommandBase<ILogger>
		{
			public static IScope<Action<ILogger>> Instance { get; } = new Scope<Action<ILogger>>( o => new Command().Execute );
			Command() : this( SystemLogger.Default.ToCommand( Defaults.Logger ).ToRunDelegate(), PurgeLoggerHistoryCommand.Default.Execute ) {}

			readonly Action assignLogger;
			readonly Action<Action<LogEvent>> purge;

			[UsedImplicitly]
			public Command( Action assignLogger, Action<Action<LogEvent>> purge )
			{
				this.assignLogger = assignLogger;
				this.purge = purge;
			}

			public override void Execute( ILogger parameter )
			{
				assignLogger();
				purge( parameter.Write );
			}
		}
	}
}