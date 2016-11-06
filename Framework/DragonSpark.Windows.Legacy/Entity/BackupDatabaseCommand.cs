using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Windows.FileSystem;
using JetBrains.Annotations;
using System;
using System.Collections.Immutable;
using System.Linq;
using Path = DragonSpark.Windows.FileSystem.Path;

namespace DragonSpark.Windows.Legacy.Entity
{
	[ApplyAutoValidation, ApplySpecification( typeof(FileSystemInfoExistsSpecification) )]
	public sealed class BackupDatabaseCommand : CommandBase<IFileInfo>
	{
		readonly IPath path;
		readonly Func<IFileInfo, bool> lockedSource;
		readonly Func<string> pathSource;
		readonly Func<string, bool> validSource;
		readonly Func<IFileInfo, ImmutableArray<IFileInfo>> fileSource;

		public BackupDatabaseCommand() : this( Path.Default, LockedFileSpecification.Default.IsSatisfiedBy, TimestampNameFactory.Default.Get, TimestampNameSpecification.Default.IsSatisfiedBy, DatabaseFiles.Default.Get ) {}

		public BackupDatabaseCommand( IPath path, Func<IFileInfo, bool> lockedSource, Func<string> pathSource, Func<string, bool> validSource, Func<IFileInfo, ImmutableArray<IFileInfo>> fileSource  )
		{
			this.lockedSource = lockedSource;
			this.pathSource = pathSource;
			this.validSource = validSource;
			this.fileSource = fileSource;
			this.path = path;
		}

		[Default( 6 ), PostSharp.Patterns.Contracts.NotNull, UsedImplicitly]
		public int? MaximumBackups { get; set; }

		public override void Execute( IFileInfo parameter )
		{
			var directory = parameter.Directory;
			var files = fileSource( parameter ).Where( lockedSource ).ToArray();
			if ( files.Any() )
			{
				var destination = directory.CreateSubdirectory( pathSource() );
				foreach ( var file in files )
				{
					file.CopyTo( path.Combine( destination.FullName, file.Name ) );
				}
			}

			if ( MaximumBackups.HasValue )
			{
				directory
					.GetDirectories()
					.Where( x => validSource( x.Name ) )
					.OrderByDescending( info => info.CreationTime )
					.Skip( MaximumBackups.Value )
					.Each( info => info.Delete( true ) );
			}
		}
	}
}