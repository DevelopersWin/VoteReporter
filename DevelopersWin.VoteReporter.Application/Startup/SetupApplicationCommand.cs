using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ServiceProviderFactory : DragonSpark.Setup.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( new DragonSpark.Activation.IoC.ServiceProviderFactory( AssemblyProvider.Instance.Create() ) ) {}
	}
}
