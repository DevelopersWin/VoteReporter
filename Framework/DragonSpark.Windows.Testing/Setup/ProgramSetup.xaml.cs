using System.Composition;
using DragonSpark.Testing.Framework.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( provider => new Application<ProgramSetup>( provider ) ) {}
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
