using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Windows.Diagnostics;
using DragonSpark.Windows.Runtime;
using Microsoft.Practices.ServiceLocation;

namespace DevelopersWin.VoteReporter
{
	public class ServiceLocator : ServiceLocatorFactory<AssemblyProvider, Logger>
	{
		public static IServiceLocator Instance { get; } = new ServiceLocator().Create();
	}

	public class Logger : CompositeMessageLogger
	{
		public Logger() : base( new TextMessageLogger(), new TraceMessageLogger() ) {}
	}
}
