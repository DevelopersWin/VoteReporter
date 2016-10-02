using DragonSpark.ComponentModel;
using System;

namespace DevelopersWin.VoteReporter
{
	public abstract class ViewBase
	{
		[NewGuid]
		public Guid Id { get; set; }

		[CurrentTime]
		public DateTimeOffset Created { get; set; }
	}
}