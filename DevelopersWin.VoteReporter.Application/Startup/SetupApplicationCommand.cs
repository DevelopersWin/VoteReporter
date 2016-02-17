using DragonSpark.Activation.FactoryModel;
using DragonSpark.Setup;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class SetupApplicationCommand : SetupApplicationCommand<Setup> {}

	[Discoverable]
	public class UnityContainerFactory : DragonSpark.Activation.IoC.UnityContainerFactory
	{}
}
