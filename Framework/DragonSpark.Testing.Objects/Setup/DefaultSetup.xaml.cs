using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;

namespace DragonSpark.Testing.Objects.Setup
{
	public partial class DefaultSetup
	{
		public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( SetupFixtureFactory<DefaultSetup>.Instance.Create ) {}
		}

		public DefaultSetup()
		{
			InitializeComponent();
		}
	}

	public class Application<T> : AutoDataApplication<T> where T : ISetup {}

	public class SetupFixtureFactory<TSetup> : FixtureFactory<ApplicationSetupCustomization<TSetup>> where TSetup : class, ISetup {}

	public class ApplicationSetupCustomization<TSetup> : Framework.Setup.ApplicationSetupCustomization<Application<TSetup>, TSetup> where TSetup : class, ISetup {}
}
