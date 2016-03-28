using DragonSpark.Activation.IoC;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ServiceProviderFactory : DragonSpark.Activation.IoC.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( 
			new IntegratedUnityContainerFactory( AssemblyProvider.Instance.Create() ).Create
		) {}
	}
}
