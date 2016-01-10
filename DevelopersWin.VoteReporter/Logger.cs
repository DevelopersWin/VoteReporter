using DragonSpark.Diagnostics;
using DragonSpark.Setup.Commands;
using DragonSpark.Windows.Diagnostics;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter
{
	public class SetupApplicationCommand : SetupApplicationCommandBase<Logger>
	{}

	public class SetupUnityCommand : SetupUnityCommand<AssemblyProvider>
	{}

	public class Logger : CompositeMessageLogger
	{
		public Logger() : base( new TextMessageLogger(), new TraceMessageLogger() ) {}
	}
}
