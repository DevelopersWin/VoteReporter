using DragonSpark.Commands;
using DragonSpark.Specifications;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class InitializeDiagnosticsCommand : SpecificationCommand<IProject>
	{
		static InitializeDiagnosticsCommand()
		{
			new CompositeCommand( DiagnosticsConfigurationCommands.Default ).Execute();
		}

		public static InitializeDiagnosticsCommand Default { get; } = new InitializeDiagnosticsCommand();
		InitializeDiagnosticsCommand() : base( 
			ContainsConfigurationSpecification.Default.And( new OncePerParameterSpecification<IProject>() ), new CompositeCommand<IProject>( ProjectConfigurationCommands.Default ).Execute
		) {}
	}
}