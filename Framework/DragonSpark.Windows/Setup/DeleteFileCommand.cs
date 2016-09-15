using DragonSpark.Commands;
using System.IO;
using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;

namespace DragonSpark.Windows.Setup
{
	[ApplyAutoValidation, Specification( typeof(FileSystemInfoExistsSpecification) )]
	public sealed class DeleteFileCommand : CommandBase<FileSystemInfo>
	{
		public static DeleteFileCommand Default { get; } = new DeleteFileCommand()/*.ExtendUsing( FileSystemInfoExistsSpecification.Default ).Extend( AutoValidationExtension.Default )*/;
		DeleteFileCommand() {}

		public override void Execute( FileSystemInfo parameter ) => parameter.Delete();
	}
}