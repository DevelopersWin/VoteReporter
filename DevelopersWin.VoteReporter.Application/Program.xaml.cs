using DragonSpark.Extensions;

namespace DevelopersWin.VoteReporter.Application
{
	/// <summary>
	/// Interaction logic for Program.xaml
	/// </summary>
	public partial class Program
	{
		static void Main( string[] args )
		{
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
