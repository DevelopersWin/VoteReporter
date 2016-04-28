using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace DragonSpark.Windows
{
	public partial class Configure
	{
		static Configure Instance { get; } = new Configure();

		[ModuleInitializer( 0 )]
		public static void Execute()
		{
			PostSharpEnvironment.IsPostSharpRunning.IsFalse( Instance.Run );
		}

		Configure()
		{
			InitializeComponent();
		}
	}
}
