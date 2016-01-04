using DragonSpark.Diagnostics;
using DragonSpark.Setup.Commands;
using DragonSpark.Windows.Diagnostics;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter
{
	public class ApplicationSetup : ApplicationSetup<Logger, AssemblyProvider>
	{ }

	public class Logger : CompositeMessageLogger
	{
		public Logger() : base( new TextMessageLogger(), new TraceMessageLogger() )
		{}
	}
}
