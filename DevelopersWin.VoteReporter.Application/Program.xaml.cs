using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application
{
	/// <summary>
	/// Interaction logic for Program.xaml
	/// </summary>
	public partial class Program
	{
		static void Main( string[] args ) =>
			ApplicationFactory<Program>.Default
									   .Create( FileSystemTypes.Default )
									   .Run( args );

		public Program() 
		{
			InitializeComponent();
		}
	}
}
