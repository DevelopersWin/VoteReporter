using DragonSpark.Application.Setup;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.Runtime;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Composition;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Windows
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Command.Default.Run();

		sealed class Command : Application.Setup.Setup
		{
			public static ICommand Default { get; } = new Command();
			Command() : base( 
				Application.Execution.Context.Configured( ExecutionContext.Default ), 
				DragonSpark.TypeSystem.Configuration.AssemblyLoader.Configured( new Func<string, Assembly>( Assembly.LoadFile ).Wrap() ),
				DragonSpark.TypeSystem.Configuration.AssemblyPathLocator.Configured( AssemblyLocator.Default.ToSourceDelegate().Wrap() )
				) {}
		}
	}

	[Export( typeof(ISetup) )]
	public class InitializationCommand : Application.Setup.Setup
	{
		public InitializationCommand() : base(
			DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.Configured( TypeDefinitionProviderSource.Default.ToFixedDelegate() ),
			DragonSpark.TypeSystem.Configuration.ApplicationAssemblyLocator.Configured( ApplicationAssemblyLocator.Default.ToSourceDelegate().Fix() )
			)
		{
			Priority = Priority.BeforeNormal;
		}
	}
}