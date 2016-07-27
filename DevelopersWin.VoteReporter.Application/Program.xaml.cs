using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Windows.Setup;

namespace DevelopersWin.VoteReporter.Application
{
	/// <summary>
	/// Interaction logic for Program.xaml
	/// </summary>
	public partial class Program
	{
		static void Main( string[] args )
		{
			var program = ApplicationFactory<Program>.Instance.Create();
			program.Run<ConsoleApplication, string[]>( args );
			// new Program().AsExecuted( args );
		}

		public Program() 
		{
			InitializeComponent();
		}
	}
}
