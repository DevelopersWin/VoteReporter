using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Data.Entity;

namespace DragonSpark.Windows.Entity
{
	public class DbMigrationsConfiguration<TContext> : System.Data.Entity.Migrations.DbMigrationsConfiguration<TContext> where TContext : DbContext
	{
		public DbMigrationsConfiguration() : this( GlobalServiceProvider.Instance.Get<ActivationSource>() ?? ActivationSource.Default ) {}

		public DbMigrationsConfiguration( [Required]IActivationSource source )
		{
			source.Apply( this );
		}
	}
}