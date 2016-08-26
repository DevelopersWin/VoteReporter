using DragonSpark.Application.Setup;
using System.Composition;

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
