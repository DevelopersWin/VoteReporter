using System.Collections.Generic;
using System.Data.Entity;
using DevelopersWin.VoteReporter.Entity;
using DragonSpark.Composition;
using DragonSpark.Sources;

namespace DevelopersWin.VoteReporter.Parts.Development
{
	public class Mappings : ItemSourceBase<ExportMapping>
	{
		public static Mappings Default { get; } = new Mappings();
		Mappings() {}

		protected override IEnumerable<ExportMapping> Yield()
		{
			yield return new ExportMapping<DropCreateDatabaseIfModelChanges<VotingContext>, IDatabaseInitializer<VotingContext>>();
		}
	}
}