using DevelopersWin.VoteReporter.Entity;
using DragonSpark.Extensions;
using System;
using System.Composition;
using System.Linq;

namespace DevelopersWin.VoteReporter.Parts.Development
{
	[Export( typeof(IVoteCountLocator) )]
	class VoteCountLocator : IVoteCountLocator
	{
		public int Locate( Vote vote )
		{
			var minimum = vote.Records.OrderByDescending( record => record.Created ).FirstOrDefault().With( x => x.Count );
			var result = new Random().Next( minimum + 5, minimum + 150 );
			return result;
		}
	}
}