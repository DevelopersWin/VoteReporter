using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Parts.Development
{
	public partial class Initialize
	{
		[ModuleInitializer( 0 ), Aspects.Runtime, AssemblyInitialize]
		public static void Execute() => new Initialize().Run();

		Initialize()
		{
			InitializeComponent();
		}
	}
}
