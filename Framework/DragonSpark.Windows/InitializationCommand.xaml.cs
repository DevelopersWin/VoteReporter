using DragonSpark.Activation;
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
		public static void Execution() => ExecutionContextRepository.Instance.Add( ExecutionContextStore.Instance );

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
		/*
		public static InitializationCommand Instance { get; } = new InitializationCommand();*/

		public InitializationCommand() : base( DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.From( Runtime.TypeDefinitionProviderStore.Instance ) )
		{
			Priority = Priority.BeforeNormal;
			// InitializeComponent();
		}
	}

	[Priority( Priority.AfterNormal )]
	class ExecutionContextStore : Source<AppDomain>, IExecutionContextStore
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();
		ExecutionContextStore() : base( AppDomain.CurrentDomain ) {}
	}
}
