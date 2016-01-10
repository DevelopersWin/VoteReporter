using DragonSpark.Setup.Registration;
using System.Data.Entity;

namespace DevelopersWin.VoteReporter.Entity
{
	[Register.As( typeof(IDatabaseInitializer<VotingContext>) )]
	public class DatabaseInitializer : DragonSpark.Windows.Entity.MigrateDatabaseToLatestVersion<VotingContext, Configuration>
	{}
}