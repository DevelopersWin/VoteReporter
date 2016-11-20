using DragonSpark.Commands;
using DragonSpark.Specifications;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class InitializeDiagnosticsCommand : CompositeCommand<IProject>
	{
		public static InitializeDiagnosticsCommand Default { get; } = new InitializeDiagnosticsCommand();
		InitializeDiagnosticsCommand() : base( 
			new SpecificationCommand<object>( new OnlyOnceSpecification(), new CompositeCommand( DiagnosticsConfigurationCommands.Default ).Execute ),
			new SpecificationCommand<IProject>( ContainsConfigurationSpecification.Default.And( new OncePerParameterSpecification<IProject>() ), new CompositeCommand<IProject>( ProjectConfigurationCommands.Default ).Execute )
		) {}
	}
}