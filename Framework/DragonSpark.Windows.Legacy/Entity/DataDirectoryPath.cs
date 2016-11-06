using DragonSpark.Windows.Runtime;
using JetBrains.Annotations;
using System.Composition;

namespace DragonSpark.Windows.Legacy.Entity
{
	public sealed class DataDirectoryPath : AppDomainStore<string>
	{
		[UsedImplicitly]
		public const string Key = "DataDirectory";

		[Export]
		public static DataDirectoryPath Default { get; } = new DataDirectoryPath();
		DataDirectoryPath() : this( @".\App_Data" ) {}

		[UsedImplicitly]
		public DataDirectoryPath( string directory ) : base( Key )
		{
			Assign( directory );
		}
	}
}