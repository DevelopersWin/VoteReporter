using DragonSpark.Commands;

namespace DevelopersWin.VoteReporter.Application
{
	public sealed partial class Program
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
