using DragonSpark.Runtime;
using DragonSpark.Runtime.Sources;
using DragonSpark.Setup;
using System;
using System.Collections.Immutable;
using System.Composition;
using DragonSpark.ComponentModel;

namespace DragonSpark.Windows
{
	[Export( typeof(ISetup) )]
	public partial class InitializationCommand
	{
		public InitializationCommand() : base( DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.Configured<ImmutableArray<ITypeDefinitionProvider>>( Runtime.TypeDefinitionProviderStore.Instance ) )
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
