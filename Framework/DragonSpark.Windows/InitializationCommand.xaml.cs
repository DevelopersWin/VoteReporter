using DragonSpark.Configuration;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using System;
using System.Composition;

namespace DragonSpark.Windows
{
	[Export( typeof(ISetup) )]
	public partial class InitializationCommand
	{
		public InitializationCommand() : base( DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.From( Runtime.TypeDefinitionProviderStore.Instance ) )
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
