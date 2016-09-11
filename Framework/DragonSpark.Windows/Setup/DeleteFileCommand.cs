using DragonSpark.Aspects.Extensibility;
using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Commands;
using System.IO;

namespace DragonSpark.Windows.Setup
{
	public sealed class DeleteFileCommand : ExtensibleCommandBase<FileSystemInfo>
	{
		public static DeleteFileCommand Default { get; } = new DeleteFileCommand().ExtendUsing( FileSystemInfoExistsSpecification.Default ).Extend( AutoValidationExtension.Default );
		DeleteFileCommand() {}

		public override void Execute( FileSystemInfo parameter ) => parameter.Delete();
	}
}