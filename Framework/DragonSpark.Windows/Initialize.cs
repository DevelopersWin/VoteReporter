using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using System;
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
}