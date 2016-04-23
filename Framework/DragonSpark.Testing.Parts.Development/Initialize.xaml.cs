using DragonSpark.Extensions;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Parts.Development
{
	public partial class Initialize
	{
		public static Initialize Instance { get; } = new Initialize();

		[ModuleInitializer( 0 )]
		public static void Execute() => Instance.Run();

		Initialize()
		{
			InitializeComponent();
		}
	}
}
