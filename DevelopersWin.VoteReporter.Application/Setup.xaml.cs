using DragonSpark.Application.Setup;
using System.Composition;

namespace DevelopersWin.VoteReporter.Application
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
