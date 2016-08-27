using DragonSpark.Application.Setup;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.Runtime;
using System.Composition;

namespace DragonSpark.Windows
{
	[Export( typeof(ISetup) )]
	public class InitializationCommand : Application.Setup.Setup
	{
		public InitializationCommand() : base( Priority.BeforeNormal,
			DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.Configured( TypeDefinitionProviderSource.Default.ToFixedDelegate() ),
			DragonSpark.TypeSystem.Configuration.ApplicationAssemblyLocator.Configured( ApplicationAssemblyLocator.Default.ToSourceDelegate().Global() )
		)
		{}
	}
}