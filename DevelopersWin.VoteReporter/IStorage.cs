using DragonSpark.Runtime.Data;
using DragonSpark.Windows.FileSystem;
using System;
using DirectoryInfo = System.IO.DirectoryInfo;
using File = System.IO.File;
using Path = System.IO.Path;

namespace DevelopersWin.VoteReporter
{
	public interface IStorage
	{
		Uri Save( object item, string fileName = null );
	}

	public sealed class Storage : IStorage
	{
		readonly ISerializer serializer;
		readonly DirectoryInfo directory;

		public Storage( ISerializer serializer, DirectoryInfo directory )
		{
			this.serializer = serializer;
			this.directory = directory;
		}

		public Uri Save( object item, string fileName = null )
		{
			var extension = item is string ? "txt" : "xaml";
			var path = Path.Combine( directory.FullName, fileName ?? $"{TimestampNameFactory.Default.Get()}.{extension}" );
			var content = item as string ?? serializer.Save( item );
			File.WriteAllText( path, content );

			var result = new Uri( path );
			return result;
		}
	}
}