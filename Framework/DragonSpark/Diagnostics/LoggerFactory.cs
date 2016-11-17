using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using JetBrains.Annotations;
using Serilog;
using System.Collections.Generic;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerConfigurationSource : Scope<LoggerConfiguration>
	{
		public static LoggerConfigurationSource Default { get; } = new LoggerConfigurationSource();
		LoggerConfigurationSource() : this( DefaultImplementation.Instance ) {}

		[UsedImplicitly]
		public LoggerConfigurationSource( IEnumerable<IAlteration<LoggerConfiguration>> configurations ) : this( new DefaultImplementation( configurations ) ) {}

		LoggerConfigurationSource( ISource<LoggerConfiguration> source ) : base( source.Get ) {}

		[UsedImplicitly]
		sealed class DefaultImplementation : AggregateSource<LoggerConfiguration>
		{
			public static DefaultImplementation Instance { get; } = new DefaultImplementation();
			DefaultImplementation() : this( LoggerConfigurations.Default ) {}

			public DefaultImplementation( IEnumerable<IAlteration<LoggerConfiguration>> configurations ) : base( configurations ) {}
		}
	}

	[UsedImplicitly]
	public sealed class CreateLogger : DelegatedParameterizedSource<LoggerConfiguration, ILogger>
	{
		public static CreateLogger Instance { get; } = new CreateLogger();
		CreateLogger() : base( configuration => configuration.CreateLogger() ) {}
	}
}