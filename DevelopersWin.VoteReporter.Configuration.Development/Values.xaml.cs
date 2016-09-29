using DragonSpark.Configuration;
using System.Composition;

namespace DevelopersWin.VoteReporter.Parts.Development
{
	[Export( typeof(IValueStore) ), Shared]
	public partial class Values
	{
		public Values()
		{
			InitializeComponent();
		}
	}
}
