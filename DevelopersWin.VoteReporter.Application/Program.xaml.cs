using DragonSpark.Setup;

namespace DevelopersWin.VoteReporter.Application
{
	public partial class Program
	{
		public Program()
		{
			InitializeComponent();
		}

		static void Main( string[] args )
		{
			new Program().Run( new SetupParameter<string[]>( args ) );
		}
	}
}
