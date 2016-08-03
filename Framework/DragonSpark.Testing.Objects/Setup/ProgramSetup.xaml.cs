using System.Composition;
using DragonSpark.Setup;

namespace DragonSpark.Testing.Objects.Setup
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
