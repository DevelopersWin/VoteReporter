using DragonSpark.Setup;
using System.Composition;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class SetupApplicationCommand : SetupApplicationCommand<Setup> {}

	[Export]
	public class UnityContainerFactory : DragonSpark.Activation.IoC.UnityContainerFactory {}
}
