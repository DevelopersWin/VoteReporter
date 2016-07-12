using DragonSpark.Windows.Setup;
using System;
using System.Composition;

namespace DragonSpark.Windows.Modularity
{
	[Export, Shared]
	public class ModulesConfigurationSectionFactory : ConfigurationSectionFactory<ModulesConfigurationSection>
	{
		public static ModulesConfigurationSectionFactory Instance { get; } = new ModulesConfigurationSectionFactory();
		ModulesConfigurationSectionFactory() {}

		public ModulesConfigurationSectionFactory( Func<string, object> factory ) : base( factory ) {}
	}

	/// <summary>
	/// Defines a store for the module metadata.
	/// </summary>
	public class ConfigurationStore : IConfigurationStore
	{
		readonly ModulesConfigurationSection section;

		public ConfigurationStore() : this( ModulesConfigurationSectionFactory.Instance.Create() ) {}

		public ConfigurationStore( ModulesConfigurationSection section )
		{
			this.section = section;
		}

		/// <summary>
		/// Gets the module configuration data.
		/// </summary>
		/// <returns>A <see cref="ModulesConfigurationSection"/> instance.</returns>
		public ModulesConfigurationSection RetrieveModuleConfigurationSection() => section;
	}
}