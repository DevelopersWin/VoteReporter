using DragonSpark.Sources.Parameterized;
using System.IO.Abstractions;

namespace DragonSpark.Windows.FileSystem
{
	sealed class Factory : FirstParameterConstructedSelector<FileSystemInfoBase, IFileSystemInfo>
	{
		public static Factory Default { get; } = new Factory();
		Factory() : base( typeof(DirectoryInfo), typeof(FileInfo) ) {}
	}
}