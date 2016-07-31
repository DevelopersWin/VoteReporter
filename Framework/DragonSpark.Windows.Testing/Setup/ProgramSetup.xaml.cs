using DragonSpark.Setup;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export( typeof(ISetup) )]
	public partial class ProgramSetup
	{
		/*public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			/*readonly static Func<MethodBase, IApplication> Source = ApplicationFactory<ProgramSetup>.Instance.Create;
			public AutoDataAttribute() : base( Source ) {}#1#
		}*/

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
