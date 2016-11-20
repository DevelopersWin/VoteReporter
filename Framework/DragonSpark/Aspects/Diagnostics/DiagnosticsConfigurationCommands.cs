using DragonSpark.Application;
using DragonSpark.Aspects.Configuration;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class ProjectConfigurationCommands : ItemSource<ICommand<IProject>>
	{
		public static ProjectConfigurationCommands Default { get; } = new ProjectConfigurationCommands();
		ProjectConfigurationCommands() : this( AssignProjectScopeCommand.Default, MonitorProjectCommand.Default, ConfigureSelfLog.Default ) {}

		[UsedImplicitly]
		public ProjectConfigurationCommands( params ICommand<IProject>[] items ) : base( items ) {}
	}

	public sealed class AssignProjectScopeCommand : CommandBase<IProject>
	{
		public static AssignProjectScopeCommand Default { get; } = new AssignProjectScopeCommand();
		AssignProjectScopeCommand() : this( Execution.Default ) {}

		readonly IAssignable<ISourceAware> assignable;

		public AssignProjectScopeCommand( IAssignable<ISourceAware> assignable )
		{
			this.assignable = assignable;
		}

		public override void Execute( IProject parameter ) => assignable.Assign( new Source<IProject>( parameter ) );
	}

	public sealed class ContainsConfigurationSpecification : SuppliedDelegatedSpecification<ConditionMonitor>
	{
		public static ContainsConfigurationSpecification Default { get; } = new ContainsConfigurationSpecification();
		ContainsConfigurationSpecification() : base( ConditionMonitorSpecification.Default, ConfigurationValidated.Default.Get ) {}
	}

	public sealed class ConfigurationValidated : SuppliedSource<ConditionMonitor>
	{
		public static ConfigurationValidated Default { get; } = new ConfigurationValidated();
		ConfigurationValidated() : this( new ConditionMonitor() ) {}

		[UsedImplicitly]
		public ConfigurationValidated( ConditionMonitor reference ) : base( reference ) {}
	}

	public sealed class DiagnosticsConfigurationCommands : ItemSourceBase<IRunCommand>
	{
		public static DiagnosticsConfigurationCommands Default { get; } = new DiagnosticsConfigurationCommands();
		DiagnosticsConfigurationCommands() : this( Configuration<DiagnosticsConfiguration>.Default.Get, ConfigurationValidated.Default.Get ) {}

		readonly Func<DiagnosticsConfiguration> source;
		readonly Func<ConditionMonitor> valid;

		[UsedImplicitly]
		public DiagnosticsConfigurationCommands( Func<DiagnosticsConfiguration> source, Func<ConditionMonitor> valid )
		{
			this.source = source;
			this.valid = valid;
		}

		protected override IEnumerable<IRunCommand> Yield()
		{
			var configuration = source();
			if ( configuration != null )
			{
				valid().Apply();

				yield return MinimumLevelConfiguration.Default.ToCommand( configuration.MinimumLevel );

				yield return AssignApplicationParts.Default.With( DefaultKnownApplicationTypes.Default, configuration.KnownApplicationTypes );

				yield return LoggerConfigurations.Configure.Instance.WithParameter(
					DefaultSystemLoggerConfigurations.Default,
					DefaultLoggerConfigurations.Default,
					configuration.Configurations
				);
			}
		}
	}
}