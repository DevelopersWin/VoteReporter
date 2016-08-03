using DragonSpark.Setup;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export( typeof(ISetup) )]
	public partial class UnitySetup
	{
		/*public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			/*readonly static Func<MethodBase, IApplication> Source = ApplicationFactory<UnitySetup>.Instance.Create;
			public AutoDataAttribute() : base( Source ) {}#1#
		}*/

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
