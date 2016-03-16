using DragonSpark.Composition;
using DragonSpark.Setup;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	// public class SetupApplicationCommand : SetupApplicationCommand<Setup> {}

	/*[Export]
	public class UnityContainerFactory : DragonSpark.Activation.IoC.UnityContainerFactory {}*/

	public class ApplicationServiceProviderFactory : DragonSpark.Setup.ApplicationServiceProviderFactory
	{
		public static ApplicationServiceProviderFactory Instance { get; } = new ApplicationServiceProviderFactory();

		public ApplicationServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}
}
