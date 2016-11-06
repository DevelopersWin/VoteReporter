using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.FileSystem;
using System;
using System.Collections.Generic;
using Path = DragonSpark.Windows.FileSystem.Path;

namespace DragonSpark.Windows.Legacy.Entity
{
	public sealed class DatabaseFiles : ParameterizedItemSourceBase<IFileInfo, IFileInfo>
	{
		readonly Func<IFileInfo, IFileInfo> logSource;

		public static DatabaseFiles Default { get; } = new DatabaseFiles();
		DatabaseFiles() : this( DatabaseLogLocator.Default.Get ) {}

		public DatabaseFiles( Func<IFileInfo, IFileInfo> logSource )
		{
			this.logSource = logSource;
		}

		protected override IEnumerable<IFileInfo> Yield( IFileInfo parameter ) => parameter.Append( logSource( parameter ) );
	}

	public sealed class DatabaseLogLocator : AlterationBase<IFileInfo>
	{
		public static DatabaseLogLocator Default { get; } = new DatabaseLogLocator();
		DatabaseLogLocator() : this( Path.Default, FileInfoFactory.Default.Get ) {}

		readonly IPath path;
		readonly Func<string, IFileInfo> fileSource;
		readonly string suffix;

		public DatabaseLogLocator( IPath path, Func<string, IFileInfo> fileSource, string suffix = "_log.ldf" )
		{
			this.path = path;
			this.fileSource = fileSource;
			this.suffix = suffix;
		}

		public override IFileInfo Get( IFileInfo parameter ) => 
			fileSource( path.Combine( parameter.DirectoryName ?? string.Empty, string.Concat( path.GetFileNameWithoutExtension( parameter.Name ), suffix ) ) );
	}

	public sealed class DataDirectory : SuppliedSource<string, IDirectoryInfo>
	{
		public static DataDirectory Default { get; } = new DataDirectory();
		DataDirectory() : base( DirectoryInfoFactory.Default.Get, DataDirectoryPath.Default.Get ) {}
	}
}