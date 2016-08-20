using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.Runtime;
using System;
using System.Composition;

namespace DragonSpark.Windows
{
	[Export( typeof(ISetup) )]
	public partial class InitializationCommand
	{
		public InitializationCommand() : base( 
			DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.Configured( TypeDefinitionProviderSource.Instance.ToFixedDelegate() ),
			DragonSpark.TypeSystem.Configuration.ApplicationAssemblyLocator.Configured( ApplicationAssemblyLocator.Instance.ToSourceDelegate().Fix() )
			)
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
