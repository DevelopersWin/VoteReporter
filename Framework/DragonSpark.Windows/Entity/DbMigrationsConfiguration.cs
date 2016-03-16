using System.Data.Entity;
using DragonSpark.Activation;

namespace DragonSpark.Windows.Entity
{
	public class DbMigrationsConfiguration<TContext> : System.Data.Entity.Migrations.DbMigrationsConfiguration<TContext> where TContext : DbContext
	{
		public DbMigrationsConfiguration() : this( Services.Get<ActivationSource>() )
		{}

		public DbMigrationsConfiguration( IActivationSource source )
		{
			source.Apply( this );
		}
	}
}