using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace DragonSpark.Windows.Testing
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Settings() => PostSharpEnvironment.IsPostSharpRunning.IsFalse( Properties.Settings.Default.Reset );

		[ModuleInitializer( 1 )]
		public static void Parts() => PostSharpEnvironment.IsPostSharpRunning.IsFalse( () => 
			new LoadPartAssemblyCommand( new TypeSystem.AssemblyLoader( assembly => "DragonSpark.Testing" ) ).Run( typeof(Initialize).Assembly )
		);
	}
}