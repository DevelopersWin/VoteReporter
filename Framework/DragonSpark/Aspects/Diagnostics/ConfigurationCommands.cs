using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DragonSpark.Application;
using DragonSpark.Aspects.Configuration;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using PostSharp.Extensibility;

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
			var test = KnownTypesForSerialization.Default.Get( PostSharpEnvironment.CurrentProject );
			throw new InvalidOperationException( $"WTF:{string.Join( ", ", test.Select( type => type.AssemblyQualifiedName ).Fixed())}" );

			var configuration = source();
			if ( configuration != null )
			{
				throw new InvalidOperationException( "WTF!" );
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