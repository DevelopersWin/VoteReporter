using DragonSpark.Windows.Runtime;
using System.Composition;

namespace DragonSpark.Windows.Entity
{
	[Export]
	public class DataDirectoryPath : AppDomainStore<string>
	{
		public const string Key = "DataDirectory";

		public DataDirectoryPath() : base( Key ) {}
	}
}