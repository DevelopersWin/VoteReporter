using DragonSpark.Aspects;
using DragonSpark.Windows.Legacy.Entity;

namespace DevelopersWin.VoteReporter.Entity
{
	[ApplyValuesFromSource]
	public class MigrationsConfiguration : DbMigrationsConfiguration<VotingContext> {}
}