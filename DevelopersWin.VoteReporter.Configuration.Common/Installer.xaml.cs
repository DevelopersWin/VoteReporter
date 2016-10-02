using DragonSpark.Windows.Entity;
using System.Composition;

namespace DevelopersWin.VoteReporter.Parts.Common
{
	/// <summary>
	/// Interaction logic for Installer.xaml
	/// </summary>
	[Export( typeof(IInstaller) )]
	public partial class Installer
	{
		public Installer()
		{
			InitializeComponent();
		}
	}
}
