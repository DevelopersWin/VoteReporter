namespace DevelopersWin.VoteReporter
{
	public interface IVoteReporter
	{
		void Report();
	}

	public sealed class VoteReporter : IVoteReporter
	{
		readonly IVoteReportGenerator generator;
		readonly IVoteReportLauncher launcher;

		public VoteReporter( IVoteReportGenerator generator, IVoteReportLauncher launcher )
		{
			this.generator = generator;
			this.launcher = launcher;
		}

		public void Report()
		{
			var location = generator.Generate();
			launcher.Launch( location );
		}
	}
}