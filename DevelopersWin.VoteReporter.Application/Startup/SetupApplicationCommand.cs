using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ServiceProviderFactory : DragonSpark.Activation.IoC.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( 
			new AssemblyBasedConfigurationContainerFactory( AssemblyProvider.Instance.Create() ).Create
		) {}
	}
}
