using System.IO;
using DragonSpark.Specifications;

namespace DragonSpark.Windows.Setup
{
	public sealed class FileSystemInfoExistsSpecification : SpecificationBase<FileSystemInfo>
	{
		public static FileSystemInfoExistsSpecification Default { get; } = new FileSystemInfoExistsSpecification();
		FileSystemInfoExistsSpecification() {}

		public override bool IsSatisfiedBy( FileSystemInfo parameter )
		{
			parameter.Refresh();
			return parameter.Exists;
		}
	}
}