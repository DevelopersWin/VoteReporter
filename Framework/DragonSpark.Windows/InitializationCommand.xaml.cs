using DragonSpark.Configuration;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Composition;
using System.Reflection;

namespace DragonSpark.Windows
{
	public static class Initialization
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Activation.Execution.Context.Assign( ExecutionContext.Instance );

		[ModuleInitializer( 1 )]
		public static void Configuration()
		{
			DragonSpark.TypeSystem.Configuration.AssemblyLoader.Assign( Assembly.LoadFile );
			DragonSpark.TypeSystem.Configuration.AssemblyPathLocator.Assign( AssemblyLocator.Instance.Get );
		}
	}

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
