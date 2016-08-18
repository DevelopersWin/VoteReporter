using DragonSpark.Runtime;
using DragonSpark.Setup;
using System;
using System.Collections.Immutable;
using System.Composition;
using DragonSpark.ComponentModel;
using DragonSpark.Sources;

namespace DragonSpark.Windows
{
	[Export( typeof(ISetup) )]
	public partial class InitializationCommand
	{
		public InitializationCommand() : base( DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.Configured<ImmutableArray<ITypeDefinitionProvider>>( Runtime.TypeDefinitionProviderSource.Instance ) )
		{
			Priority = Priority.BeforeNormal;
			// InitializeComponent();
		}
	}

	[Priority( Priority.AfterNormal )]
	class ExecutionContext : Source<AppDomain>
	{
		public static ISource Instance { get; } = new ExecutionContext();
		ExecutionContext() : base( AppDomain.CurrentDomain ) {}
	}
}
