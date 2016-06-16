using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Windows.Properties;
using System.IO;

namespace DragonSpark.Windows.Entity
{
	public class InstallDatabaseCommand : CommandBase<object>
	{
		[Factory( typeof(AttachedDatabaseFileFactory) )]
		public FileInfo Database { get; set; }

		public override void Execute( object parameter ) => Database.Exists.IsFalse( () =>
		{
			var items = EntityFiles.WithLog( Database ).Tuple( new[] { Resources.Blank, Resources.Blank_log } );
			items.Each( tuple => 
			{
				using ( var stream = File.Create( tuple.Item1.FullName ) )
				{
					stream.Write( tuple.Item2, 0, tuple.Item2.Length );
				}
			} );
		} );
	}
}