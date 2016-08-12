using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Windows
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Command.Instance.Run();

		class Command : DragonSpark.Setup.Setup
		{
			public static ICommand Instance { get; } = new Command();
			Command() : base( 
				Activation.Execution.Context.Configured( ExecutionContext.Instance ), 
				DragonSpark.TypeSystem.Configuration.AssemblyLoader.Configured( new Func<string, Assembly>( Assembly.LoadFile ).Wrap() ),
				DragonSpark.TypeSystem.Configuration.AssemblyPathLocator.Configured( AssemblyLocator.Instance.ToDelegate().Wrap() )
				) {}
		}
	}
}