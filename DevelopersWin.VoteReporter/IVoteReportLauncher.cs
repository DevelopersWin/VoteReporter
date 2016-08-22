using System;
using System.Diagnostics;

namespace DevelopersWin.VoteReporter
{
	public interface IVoteReportLauncher
	{
		void Launch( Uri location );
	}

	class VoteReportLauncher : IVoteReportLauncher
	{
		public void Launch( Uri location )
		{
			Process.Start( location.LocalPath );
		}
	}
}