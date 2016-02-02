using DragonSpark.Setup.Registration;
using DragonSpark.Windows.Entity;
using System.Data.Entity;

namespace DevelopersWin.VoteReporter.Entity
{
	[Register.Mapped( typeof(DbContext) )]
	public class VotingContext : EntityContext
	{
		public DbSet<Vote> Votes { get; set; }

		public DbSet<Recording> Recordings { get; set; }
	}
}