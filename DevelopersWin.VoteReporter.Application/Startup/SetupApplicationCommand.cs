using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ServiceProviderFactory : DragonSpark.Setup.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}
}
