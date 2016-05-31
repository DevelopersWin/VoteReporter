using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Aspects;

namespace DragonSpark.Windows
{
	public partial class Configure
	{
		[ModuleInitializer( 0 ), Aspects.Runtime, AssemblyInitialize]
		public static void Initialize() => new Configure().Run();

		public Configure()
		{
			InitializeComponent();
		}
	}
}
