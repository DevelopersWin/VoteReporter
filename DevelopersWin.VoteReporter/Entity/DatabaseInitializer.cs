using System.Composition;
using System.Data.Entity;

namespace DevelopersWin.VoteReporter.Entity
{
	[Export( typeof(IDatabaseInitializer<VotingContext>) )]
	public class DatabaseInitializer : DragonSpark.Windows.Entity.MigrateDatabaseToLatestVersion<VotingContext, Configuration> {}
}