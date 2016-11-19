using DragonSpark.Application;
using DragonSpark.Aspects.Configuration;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class ConfigurationCommands : ItemSourceBase<ICommand>
	{
		public static ConfigurationCommands Default { get; } = new ConfigurationCommands();
		ConfigurationCommands() : this( Configuration<DiagnosticsConfiguration>.Default.Get ) {}

		readonly Func<DiagnosticsConfiguration> source;

		public ConfigurationCommands( Func<DiagnosticsConfiguration> source )
		{
			this.source = source;
		}

		protected override IEnumerable<ICommand> Yield()
		{
			var configuration = source();
			if ( configuration != null )
			{
				yield return MinimumLevelConfiguration.Default.ToCommand( configuration.MinimumLevel );

				yield return AssignApplicationParts.Default.With( DefaultKnownApplicationTypes.Default, configuration.KnownApplicationTypes );

				yield return LoggerConfigurations.Configure.Instance.WithParameter(
													 DefaultSystemLoggerConfigurations.Default,
													 DefaultLoggerConfigurations.Default,
													 configuration.Configurations
												 );
				// yield return DisposeOnCompleteCommand.Default,
				yield return ConfigureSelfLog.Default;
			}
		}
	}
}