using DragonSpark.Runtime;
using DragonSpark.Windows.Io;
using System;
using System.IO;

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

		public Uri Save( object item, string fileName )
		{
			var extension = item is string ? "txt" : "xaml";
			var path = Path.Combine( directory.FullName, fileName ?? $"{FileSystem.GetValidPath()}.{extension}" );
			var content = item as string ?? serializer.Save( item );
			File.WriteAllText( path, content );

			var result = new Uri( path );
			return result;
		}
	}
}