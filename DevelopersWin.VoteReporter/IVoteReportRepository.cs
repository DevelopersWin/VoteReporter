namespace DevelopersWin.VoteReporter
{
	public interface IVoteReportRepository
	{
		void Save( VoteReport report );
	}

	class VoteReportRepository : IVoteReportRepository
	{
		readonly IStorage storage;

		public VoteReportRepository( IStorage storage )
		{
			this.storage = storage;
		}

		public void Save( VoteReport report )
		{
			storage.Save( report );
		}
	}
}