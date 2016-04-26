using PostSharp.Aspects;

namespace DragonSpark.Testing
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Parts()
		{
			/*PostSharpEnvironment.IsPostSharpRunning.IsFalse( () =>
																 LoadPartAssemblyCommand.Instance.Run( typeof(Initialize).Assembly )
				);*/
		}
	}
}
