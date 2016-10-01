using DragonSpark.Commands;

namespace DevelopersWin.VoteReporter.Application
{
	public sealed partial class Program
	{
		static void Main( string[] args )
		{
			/*foreach ( var command in ApplicationCommands.Default.Get() )
			{
				command.Execute( args );
			}*/

			/*var temp = Parts.Common.Properties.Settings.Default.ApiKey;
			Debugger.Break();*/

			// System.Windows.Forms.Application.LocalUserAppDataPath
			// var folder = locala

			using ( var program = new Program() )
			{
				program.Run( args );
			}
		}

		public Program()
		{
			InitializeComponent();
		}
	}
}
