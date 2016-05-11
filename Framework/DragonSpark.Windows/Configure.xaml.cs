using DragonSpark.Extensions;
using PostSharp.Aspects;

namespace DragonSpark.Windows
{
	public partial class Configure
	{
		[ModuleInitializer( 0 ), Aspects.Runtime]
		public static void Execute() => new Configure().Run();

		public Configure()
		{
			InitializeComponent();
		}
	}
}
