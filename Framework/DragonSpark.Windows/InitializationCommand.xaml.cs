using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Windows
{
	public partial class InitializationCommand
	{
		[ModuleInitializer( 0 )]
		public static void Initialize() => ExecutionContextRepository.Instance.Add( ExecutionContextStore.Instance );

		public static InitializationCommand Instance { get; } = new InitializationCommand();
		InitializationCommand()
		{
			InitializeComponent();
		}
	}

	public class ApplyAttributeProviderConfiguration : ApplyParameterizedFactoryConfigurationCommand<IAttributeProvider> {}

	[Priority( Priority.AfterNormal )]
	class ExecutionContextStore : Store<AppDomain>, IExecutionContextStore
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();
		ExecutionContextStore() : base( AppDomain.CurrentDomain ) {}
	}
}
