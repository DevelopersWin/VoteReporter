using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Setup.Commands;
using Microsoft.Practices.Unity.Configuration;
using PostSharp.Patterns.Contracts;
using System.Composition;
using System.Linq;

namespace DragonSpark.Windows.Setup
{
	[Export, Shared]
	public class UnityConfigurationSectionFactory : ConfigurationSectionFactory<UnityConfigurationSection>
	{
		public static UnityConfigurationSectionFactory Instance { get; } = new UnityConfigurationSectionFactory();

		public UnityConfigurationSectionFactory() {}
	}

	public class SetupUnityFromConfigurationCommand : UnityCommand
	{
		[Locate, Required]
		public UnityConfigurationSection Section { [return: Required]get; set; }

		public override void Execute( object parameter ) => Section.Containers.Any().IsTrue( () => Container.LoadConfiguration() );
	}
}