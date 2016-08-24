using System.IO;
using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;

namespace DragonSpark.Windows.Setup
{
	[ApplyAutoValidation]
	public sealed class DeleteFileCommand : CommandBase<FileInfo>
	{
		public static DeleteFileCommand Default { get; } = new DeleteFileCommand();
		DeleteFileCommand() : base( FileSystemInfoExistsSpecification.Default ) {}

		public override void Execute( FileInfo parameter ) => parameter.Delete();
	}
}