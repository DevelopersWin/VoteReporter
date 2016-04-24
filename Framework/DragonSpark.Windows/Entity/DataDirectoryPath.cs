using DragonSpark.Setup.Registration;
using DragonSpark.Windows.Runtime;

namespace DragonSpark.Windows.Entity
{
	[Register.Type]
	public class DataDirectoryPath : AppDomainStore<string>
	{
		public const string Key = "DataDirectory";

		public DataDirectoryPath() : base( Key )
		{}
	}
}