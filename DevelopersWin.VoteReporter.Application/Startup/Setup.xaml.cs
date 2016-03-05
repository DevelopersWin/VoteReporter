using DragonSpark.Setup;
using System.Composition;

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
