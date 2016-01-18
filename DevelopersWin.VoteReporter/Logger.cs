using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Windows.Diagnostics;
using DragonSpark.Windows.Runtime;
using Microsoft.Practices.ServiceLocation;

namespace DevelopersWin.VoteReporter
{
	public static class ServiceLocator
	{
		public static IServiceLocator Instance { get; } = ServiceLocatorFactory.Instance.Create();
	}

	public class ServiceLocatorFactory : DragonSpark.Activation.IoC.ServiceLocatorFactory
	{
		public static ServiceLocatorFactory Instance { get; } = new ServiceLocatorFactory();

		ServiceLocatorFactory() : base( UnityContainerFactory<AssemblyProvider, Logger>.Instance.Create ) {}
	}

	public class Logger : CompositeMessageLogger
	{
		public Logger() : base( new TextMessageLogger(), new TraceMessageLogger() ) {}
	}
}
