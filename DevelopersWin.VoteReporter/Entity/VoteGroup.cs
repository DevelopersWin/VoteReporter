using DragonSpark.ComponentModel;
using System.Collections.Generic;
using System.Windows.Markup;

namespace DevelopersWin.VoteReporter.Entity
{
	[ContentProperty( nameof(Votes) )]
	public class VoteGroup : VoteBase
	{
		[Collection]
		public virtual ICollection<Vote> Votes { get; set; }
	}
}