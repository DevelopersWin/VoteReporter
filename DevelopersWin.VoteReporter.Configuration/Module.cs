using DevelopersWin.VoteReporter.Entity;
using DragonSpark.Modularity;
using System.Globalization;

namespace DevelopersWin.VoteReporter.Configuration
{
	public class Module : MonitoredModule<Setup>
	{
		public Module( IModuleMonitor moduleMonitor, Setup command ) : base( moduleMonitor, command )
		{}
	}

	class VoteCountLocator : IVoteCountLocator
	{
		readonly DocumentProvider provider;

		public VoteCountLocator( DocumentProvider provider )
		{
			this.provider = provider;
		}

		public int Locate( Vote vote )
		{
			var document = provider.Load( vote.Location );
			var node = document.DocumentNode.SelectSingleNode( "//div[@class='uvIdeaVoteCount']/strong" );
			var data = node.InnerText;
			var result = int.Parse( data, NumberStyles.AllowThousands );
			return result;
		}
	}
}