using DragonSpark.Extensions;
using DragonSpark.Windows.Setup;

namespace DevelopersWin.VoteReporter.Application
{
	public partial class Program
	{
		static void Main( string[] args )
		{
			using ( var parameter = new ConsoleSetupParameter( ServiceLocator.Instance, args ) )
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
