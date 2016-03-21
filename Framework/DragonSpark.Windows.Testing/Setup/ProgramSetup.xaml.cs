using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( autoData => new Application<ProgramSetup>() ) {}
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
