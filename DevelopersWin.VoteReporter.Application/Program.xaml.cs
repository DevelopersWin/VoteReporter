using DragonSpark.Extensions;
using DragonSpark.Setup;

namespace DevelopersWin.VoteReporter.Application
{
	public partial class Program
	{
		static void Main( string[] args )
		{
			using ( var parameter = new SetupParameter<string[]>( args ) )
			{
				new Program().Run( parameter );
			}
		}

		public Program()
		{
			InitializeComponent();
		}
	}
}
