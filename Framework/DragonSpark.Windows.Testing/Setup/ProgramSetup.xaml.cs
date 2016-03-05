using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( FixtureFactory<Customization<ProgramSetup>>.Instance.Create ) {}
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
