using DragonSpark.Activation;
using System.Configuration;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Setup
{
	public class FileConfigurationFactory : ValidatedParameterizedSourceBase<string, object>
	{
		readonly ConfigurationFileMap map;

		public FileConfigurationFactory( string filePath ) : this( new ConfigurationFileMap( filePath ) )
		{ }

		public FileConfigurationFactory( ConfigurationFileMap map )
		{
			this.map = map;
		}

		public override object Get( string parameter ) => ConfigurationManager.OpenMappedMachineConfiguration( map ).GetSection( parameter );
	}
}