using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ApplicationServiceProviderFactory : DragonSpark.Setup.ApplicationServiceProviderFactory
	{
		public static ApplicationServiceProviderFactory Instance { get; } = new ApplicationServiceProviderFactory();

		public ApplicationServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}
}
