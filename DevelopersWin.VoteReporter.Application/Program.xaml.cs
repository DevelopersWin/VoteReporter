using DragonSpark.Extensions;
using System.Diagnostics;
using System.Linq;

namespace DevelopersWin.VoteReporter.Application
{
	/// <summary>
	/// Interaction logic for Program.xaml
	/// </summary>
	public partial class Program
	{
		static void Main( string[] args )
		{
			typeof(IVoteCountLocator).GetType(); // TODO: remove.
			using ( var program = new Program() )
			{
				program.Run( args );
				Debugger.Break();
			}
		}

		public Program() : base( ApplicationCommands.Default.Get().ToArray() )
		{
			InitializeComponent();
		}
	}
}
