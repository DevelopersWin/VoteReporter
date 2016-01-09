using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Entity;
using System.Data.Entity;

namespace DevelopersWin.VoteReporter.Entity
{
	[Register( typeof(DbContext) )]
	public class VotingContext : EntityContext
	{
		public VotingContext( IAttributeProvider provider ) : base( provider )
		{}

		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}

		protected override void OnModelCreating( DbModelBuilder modelBuilder )
		{
			// modelBuilder.Entity<Vote>().HasOptional(  )

			base.OnModelCreating( modelBuilder );
		}

		public DbSet<VoteGroup> Groups { get; set; }

		public DbSet<Vote> Votes { get; set; }

		public DbSet<Recording> Recordings { get; set; }
	}
}