using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Windows
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Command.Instance.Run();

		class Command : DragonSpark.Setup.Setup
		{
			public static Command Instance { get; } = new Command();
			Command() : base( 
				Activation.Execution.Context.From( ExecutionContext.Instance ), 
				DragonSpark.TypeSystem.Configuration.AssemblyLoader.From( Assembly.LoadFile ),
				DragonSpark.TypeSystem.Configuration.AssemblyPathLocator.From( AssemblyLocator.Instance.Get )
				) {}
		}
	}
}