using System.Composition;
using DragonSpark.Application.Setup;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	[Export( typeof(ISetup) )]
	public partial class Setup
	{
		public Setup()
		{
			InitializeComponent();
		}
	}
}
