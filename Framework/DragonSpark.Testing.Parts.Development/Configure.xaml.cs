using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Parts.Development
{
	public partial class Configure
	{
		[ModuleInitializer( 0 ), Aspects.Runtime, AssemblyInitialize]
		public static void Initialize() => new Configure().Run();

		Configure()
		{
			InitializeComponent();
		}
	}
}
