using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Windows.Diagnostics;
using DragonSpark.Windows.Runtime;
using Microsoft.Practices.ServiceLocation;

namespace DevelopersWin.VoteReporter
{
	public static class ServiceLocator
	{
		public static IServiceLocator Instance { get; } = new ServiceLocatorFactory().Create();
	}

	public class ServiceLocatorFactory : ServiceLocatorFactory<AssemblyProvider, Logger> {}

	public class Logger : CompositeMessageLogger
	{
		public Logger() : base( new TextMessageLogger(), new TraceMessageLogger() ) {}
	}
}
