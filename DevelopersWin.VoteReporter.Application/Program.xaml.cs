using DragonSpark.Extensions;

namespace DevelopersWin.VoteReporter.Application
{
	/// <summary>
	/// Interaction logic for Program.xaml
	/// </summary>
	public partial class Program
	{
		static void Main( string[] args ) => new Program().Run( args );

		public Program()
		{
			var temp = DevelopersWin.VoteReporter.Configuration.Development.Properties.Settings.Default.Setting;

			InitializeComponent();
		}
	}
}
