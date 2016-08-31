using DragonSpark.Sources.Parameterized;
using Serilog;
using System.Composition;

namespace DevelopersWin.VoteReporter.Parts.Development
{
	/// <summary>
	/// Interaction logic for Logger.xaml
	/// </summary>
	[Export( typeof(IAlteration<LoggerConfiguration>) ), Shared]
	public partial class Logger
	{
		public Logger()
		{
			InitializeComponent();
		}
	}
}
