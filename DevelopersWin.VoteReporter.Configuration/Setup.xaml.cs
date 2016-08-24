using System.Composition;
using DragonSpark.Application.Setup;

namespace DevelopersWin.VoteReporter.Parts
{
	/// <summary>
	/// Interaction logic for Setup.xaml
	/// </summary>
	[Export( typeof(ISetup) )]
	public partial class Setup
	{
		public Setup()
		{
			InitializeComponent();
		}
	}
}
