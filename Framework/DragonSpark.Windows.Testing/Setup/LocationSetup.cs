using DragonSpark.Setup;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export( typeof(ISetup) )]
	public partial class LocationSetup
	{
		/*public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			/*readonly static Func<MethodBase, IApplication> Source = ApplicationFactory<LocationSetup>.Instance.Create;
			public AutoDataAttribute() : base( Source ) {}#1#
		}*/

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
