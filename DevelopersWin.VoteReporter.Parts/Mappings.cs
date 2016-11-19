using DevelopersWin.VoteReporter.Entity;
using DragonSpark.Composition;
using DragonSpark.Sources;
using System.Collections.Generic;
using System.Composition;
using System.Data.Entity;

namespace DevelopersWin.VoteReporter.Parts
{
	public class Mappings : ItemSourceBase<ExportMapping>
	{
		[Export( typeof(IEnumerable<ExportMapping>) )]
		public static Mappings Default { get; } = new Mappings();
		Mappings() {}

		protected override IEnumerable<ExportMapping> Yield()
		{
			yield return new ExportMapping<DragonSpark.Windows.Legacy.Entity.MigrateDatabaseToLatestVersion<VotingContext, MigrationsConfiguration>, IDatabaseInitializer<VotingContext>>();
		}
	}
}