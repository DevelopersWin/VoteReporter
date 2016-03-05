using DragonSpark.Activation.IoC;
using System.Windows.Input;

namespace DragonSpark.Windows.Testing.Setup
{
	public class ApplicationCustomization<T> : DragonSpark.Testing.Framework.Setup.ApplicationCustomization<T> where T : ICommand
	{
		public ApplicationCustomization() : base( new AssignLocationCommand() ) {}
	}

	// public class SetupFixtureFactory<TSetup> : FixtureFactory<ApplicationSetupCustomization<TSetup>> where TSetup : class, ISetup {}

	// public class ApplicationSetupCustomization<TSetup> : ApplicationSetupCustomization<Application<TSetup>, TSetup> where TSetup : class, ISetup {}

	// public class AssignLocationCommand : AssignLocationCommand<UnityContainerFactory> {}
}
