using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Windows.FileSystem;
using DragonSpark.Windows.Legacy.Properties;
using JetBrains.Annotations;
using System;
using System.Collections.Immutable;
using System.Linq;
using Directory = DragonSpark.Windows.FileSystem.Directory;
using File = DragonSpark.Windows.FileSystem.File;
using Path = DragonSpark.Windows.FileSystem.Path;

namespace DragonSpark.Windows.Legacy.Entity
{
	[ApplyAutoValidation, ApplyInverseSpecification( typeof(FileSystemInfoExistsSpecification) )]
	public class InstallDatabaseCommand : CommandBase<IFileInfo>
	{
		readonly IPath path;
		readonly IDirectory directory;
		readonly IFile file;
		readonly Func<IFileInfo, ImmutableArray<IFileInfo>> fileSource;

		readonly static byte[][] Data = { Resources.Blank, Resources.Blank_log };

		public static InstallDatabaseCommand Default { get; } = new InstallDatabaseCommand();
		InstallDatabaseCommand() : this( Path.Default, Directory.Default, File.Default, DatabaseFiles.Default.Get ) {}

		[UsedImplicitly]
		public InstallDatabaseCommand( IPath path, IDirectory directory, IFile file, Func<IFileInfo, ImmutableArray<IFileInfo>> fileSource )
		{
			this.path = path;
			this.directory = directory;
			this.file = file;
			this.fileSource = fileSource;
		}

		public override void Execute( IFileInfo parameter )
		{
			foreach ( var item in fileSource( parameter ).Tuple( Data ).ToArray() )
			{
				var fullName = item.Item1.FullName;
				var directoryRoot = path.GetDirectoryName( fullName );
				if ( directoryRoot != null )
				{
					directory.CreateDirectory( directoryRoot );
					using ( var stream = file.Create( fullName ) )
					{
						stream.Write( item.Item2, 0, item.Item2.Length );
					}
				}
			}
		}
	}
}