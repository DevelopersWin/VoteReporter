using DragonSpark.Testing.Framework.Setup;
using System.Composition;

namespace DragonSpark.Testing.Objects.Setup
{
	[Export]
	public partial class DefaultSetup
	{
		public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( FixtureFactory<ApplicationCustomization<DefaultSetup>>.Instance.Create ) {}
		}

		public DefaultSetup()
		{
			InitializeComponent();
		}
	}
}
