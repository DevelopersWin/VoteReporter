using DragonSpark.Composition;
using DragonSpark.Setup;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	// public class SetupApplicationCommand : SetupApplicationCommand<Setup> {}

	/*[Export]
	public class UnityContainerFactory : DragonSpark.Activation.IoC.UnityContainerFactory {}*/

	public class ApplicationContextFactory : DragonSpark.Setup.ApplicationContextFactory
	{
		public static ApplicationContextFactory Instance { get; } = new ApplicationContextFactory();

		public ApplicationContextFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}
}
