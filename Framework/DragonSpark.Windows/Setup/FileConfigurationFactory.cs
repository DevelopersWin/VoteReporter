using DragonSpark.Activation;
using System.Configuration;

namespace DragonSpark.Windows.Setup
{
	public class FileConfigurationFactory : FactoryWithSpecificationBase<string, object>
	{
		readonly ConfigurationFileMap map;

		public FileConfigurationFactory( string filePath ) : this( new ConfigurationFileMap( filePath ) )
		{ }

		public FileConfigurationFactory( ConfigurationFileMap map )
		{
			this.map = map;
		}

		public override object Create( string parameter ) => ConfigurationManager.OpenMappedMachineConfiguration( map ).GetSection( parameter );
	}
}