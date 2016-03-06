using DragonSpark.Testing.Objects.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( () => new ApplicationWithLocation<ProgramSetup>() ) {}
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
