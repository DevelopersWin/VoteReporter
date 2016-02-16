using DragonSpark.Activation.IoC;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
using UnityContainerFactory = DragonSpark.Testing.Objects.Setup.UnityContainerFactory;

namespace DragonSpark.Windows.Testing.Setup
{
	public class Application<T> : AutoDataApplication<T> where T : ISetup
	{
		public Application() : base( new AssignLocationCommand() ) {}
	}

	public class SetupFixtureFactory<TSetup> : FixtureFactory<ApplicationSetupCustomization<TSetup>> where TSetup : class, ISetup {}

	public class ApplicationSetupCustomization<TSetup> : ApplicationSetupCustomization<Application<TSetup>, TSetup> where TSetup : class, ISetup {}

	public class AssignLocationCommand : AssignLocationCommand<UnityContainerFactory> {}
}
