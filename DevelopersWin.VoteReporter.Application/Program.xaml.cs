using System.Configuration;
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
			var temp = ConfigurationManager.AppSettings["Testing"];

			InitializeComponent();
		}
	}
}
