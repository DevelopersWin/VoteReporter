using DragonSpark.Configuration;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using Serilog;
using Serilog.Core;
using System.Collections.Immutable;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class Logger : LoggerBase
	{
		public static IConfigurableFactory<LoggerConfiguration, ILogger> Configurable { get; } = new Logger();

		public static IParameterizedSource<ILogger> Default { get; } = Configurable.ToCache();
		Logger() {}
	}

	public abstract class LoggerBase : ConfigurableParameterizedFactoryBase<LoggerConfiguration, ILogger>
	{
		protected LoggerBase() : this( Items<IAlteration<LoggerConfiguration>>.Default ) {}
		protected LoggerBase( params IAlteration<LoggerConfiguration>[] configurations ) : this( configurations.ToImmutableArray() ) {}
		protected LoggerBase( ImmutableArray<IAlteration<LoggerConfiguration>> configurations ) : base( o => new LoggerConfiguration(), configurations.Wrap(), ( configuration, parameter ) => configuration.CreateLogger().ForContext( Constants.SourceContextPropertyName, parameter, true ) ) {}

		/*[Freeze]
		public override ILogger Get( object parameter ) => base.Get( parameter );*/
	}

	public sealed class SystemLogger : LoggerBase
	{
		public static IConfigurableFactory<LoggerConfiguration, ILogger> Configurable { get; } = new SystemLogger();

		public static IParameterizedSource<ILogger> Default { get; } = Configurable.ToCache();
		SystemLogger() : base( DefaultSystemLoggerConfigurations.Default.Get() ) {}
	}
}