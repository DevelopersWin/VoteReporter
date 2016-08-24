using System.Configuration;
using System.IO;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Setup
{
	public class UserSettingsPathFactory : ParameterizedSourceBase<ConfigurationUserLevel, FileInfo>
	{
		public static UserSettingsPathFactory Default { get; } = new UserSettingsPathFactory();
		UserSettingsPathFactory() {}

		public override FileInfo Get( ConfigurationUserLevel parameter ) => new FileInfo( ConfigurationManager.OpenExeConfiguration( parameter ).FilePath );
	}
}