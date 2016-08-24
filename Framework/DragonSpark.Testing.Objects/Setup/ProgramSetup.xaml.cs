using System.Composition;
using DragonSpark.Application.Setup;

namespace DragonSpark.Testing.Objects.Setup
{
	[Export( typeof(ISetup) )]
	public partial class ProgramSetup
	{
		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
