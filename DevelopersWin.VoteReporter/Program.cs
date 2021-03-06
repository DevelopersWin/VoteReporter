using DragonSpark.Application;

namespace DevelopersWin.VoteReporter
{
	public sealed class Program : Program<object[]>
	{
		readonly IVoteRecorder recorder;
		readonly IVoteReporter reporter;

		public Program( IVoteRecorder recorder, IVoteReporter reporter )
		{
			this.recorder = recorder;
			this.reporter = reporter;
		}

		protected override void Run( object[] arguments )
		{
			recorder.Record();
			reporter.Report();
		}
	}
}