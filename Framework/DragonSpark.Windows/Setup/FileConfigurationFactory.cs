using DragonSpark.Activation;
using System.Configuration;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Setup
{
	public class FileConfigurationFactory : FactoryBase<string, object>
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