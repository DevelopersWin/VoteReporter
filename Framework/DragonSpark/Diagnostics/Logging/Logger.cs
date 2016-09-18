using DragonSpark.Configuration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Serilog;
using Serilog.Core;

namespace DragonSpark.Diagnostics.Logging
{
	public sealed class Logger : ConfigurableParameterizedFactoryBase<LoggerConfiguration, ILogger>
	{
		public static IParameterizedSource<ILogger> Default { get; } = new Logger().ToCache();
		Logger() : base( o => new LoggerConfiguration(), LoggerConfigurationSource.Default.ToDelegate().Wrap(), ( configuration, parameter ) => configuration.CreateLogger().ForContext( Constants.SourceContextPropertyName, parameter, true ) ) {}
	}
}