using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SqlClient;
using System.IO;

namespace DragonSpark.Windows.Entity
{
	public class AttachedDatabaseFileFactory : FactoryBase<FileInfo>
	{
		readonly DbContext context;

		public AttachedDatabaseFileFactory( DbContext context )
		{
			this.context = context;
		}

		public override FileInfo Create() => 
			new SqlConnectionStringBuilder( context.Database.Connection.ConnectionString ).AttachDBFilename.NullIfEmpty().With( DbProviderServices.ExpandDataDirectory ).With( s => new FileInfo( s ) );
	}
}