using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

namespace DragonSpark.Diagnostics
{
	public sealed class Logger : ParameterizedSingletonScope<object, ILogger>
	{
		public static Logger Default { get; } = new Logger();
		Logger() : base( o => new Implementation().Get ) {}

		public sealed class Implementation : ParameterizedSourceBase<ILogger>
		{
			public Implementation() : this( LoggerFactory.Default.Get(), ObjectTypeCoercer.Default.Get, new CompositeAssignableSpecification( FormattableTypes.Default.Get() ).ToCachedSpecification().IsSatisfiedBy ) {}

			readonly ILogger logger;
			readonly Func<object, Type> coerce;
			readonly Func<Type, bool> specification;

			[UsedImplicitly]
			public Implementation( ILogger logger, Func<object, Type> coerce, Func<Type, bool> specification )
			{
				this.logger = logger;
				this.coerce = coerce;
				this.specification = specification;
			}

			public override ILogger Get( object parameter )
			{
				var type = coerce( parameter );
				var supported = specification( type );
				var context = supported ? parameter : type;
				var result = logger.ForContext( Constants.SourceContextPropertyName, context, supported );
				return result;
			}
		}
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