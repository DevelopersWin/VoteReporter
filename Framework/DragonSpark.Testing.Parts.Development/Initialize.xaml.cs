using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace DragonSpark.Testing.Parts.Development
{
	public partial class Initialize
	{
		public static Initialize Instance { get; } = new Initialize();

		[ModuleInitializer( 0 )]
		public static void Execute() => PostSharpEnvironment.IsPostSharpRunning.IsFalse( Instance.Run );

		Initialize()
		{
			InitializeComponent();
		}
	}
}
