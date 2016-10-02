using DragonSpark.Windows.Entity;
using System.Composition;
using System.Data.Entity;

namespace DevelopersWin.VoteReporter.Entity
{
	[Export( typeof(DbContext) )]
	public class VotingContext : EntityContext
	{
		public DbSet<Vote> Votes { get; set; }

		public DbSet<Recording> Recordings { get; set; }
	}
}