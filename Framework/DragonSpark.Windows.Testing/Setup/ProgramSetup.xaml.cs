using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			/*readonly static Func<MethodBase, IApplication> Source = ApplicationFactory<ProgramSetup>.Instance.Create;
			public AutoDataAttribute() : base( Source ) {}*/
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
