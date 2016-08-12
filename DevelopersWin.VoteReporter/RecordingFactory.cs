using DevelopersWin.VoteReporter.Entity;
using DragonSpark.Extensions;
using DragonSpark.Windows.Entity;
using PostSharp.Patterns.Contracts;
using System.Linq;
using DragonSpark.Sources;

namespace DevelopersWin.VoteReporter
{
	public class RecordingFactory : SourceBase<Recording>
	{
		readonly VotingContext context;
		readonly IVoteProvider provider;
		readonly IVoteUpdater updater;

		public RecordingFactory( [Required]VotingContext context, [Required]IVoteProvider provider, [Required]IVoteUpdater updater )
		{
			this.context = context;
			this.provider = provider;
			this.updater = updater;
		}

		public override Recording Get()
		{
			var result = context.Create<Recording>();
			var votes = provider.Retrieve( result ).ToArray();
			result.Records = votes.Select( vote => vote.With( x => updater.Update( result, vote ) ).Records.OrderByDescending( record => record.Created ).First() ).ToList();
			return result;
		}
	}
}