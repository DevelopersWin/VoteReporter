using DragonSpark.Configuration;
using System.Composition;

namespace DevelopersWin.VoteReporter.Parts.Common
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
