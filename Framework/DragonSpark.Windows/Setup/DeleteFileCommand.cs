using DragonSpark.Commands;
using System.IO;

namespace DragonSpark.Windows.Setup
{
	// [ApplyAutoValidation]
	public sealed class DeleteFileCommand : CommandBase<FileInfo>
	{
		public static ICommand<FileInfo> Default { get; } = new DeleteFileCommand().Apply( FileSystemInfoExistsSpecification.Default );
		DeleteFileCommand() {}

		public override void Execute( FileInfo parameter ) => parameter.Delete();
	}
}