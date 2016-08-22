using DragonSpark.Activation;
using System.Data.Entity;

namespace DragonSpark.Windows.Entity
{
	public class MigrateDatabaseToLatestVersion<TContext, TConfiguration> : System.Data.Entity.MigrateDatabaseToLatestVersion<TContext, TConfiguration> where TContext : DbContext where TConfiguration : DbMigrationsConfiguration<TContext>, new()
	{
		public MigrateDatabaseToLatestVersion() : this( false ) {}

		public MigrateDatabaseToLatestVersion( bool useSuppliedContext ) : base( useSuppliedContext, GlobalServiceProvider.GetService<TConfiguration>() ) {}
	}
}