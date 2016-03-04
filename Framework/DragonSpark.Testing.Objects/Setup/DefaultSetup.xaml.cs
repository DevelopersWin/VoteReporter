using DragonSpark.ComponentModel;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using System.Reflection;

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

	public class Application<T> : AutoDataApplication<T> where T : ISetup
	{
		[Value( typeof(AssemblyHost) )]
		public override Assembly[] Assemblies
		{
			get { return base.Assemblies; }
			set { base.Assemblies = value; }
		}
	}

	public class SetupFixtureFactory<TSetup> : FixtureFactory<ApplicationSetupCustomization<TSetup>> where TSetup : class, ISetup {}

	public class ApplicationSetupCustomization<TSetup> : ApplicationSetupCustomization<Application<TSetup>, TSetup> where TSetup : class, ISetup {}
}
